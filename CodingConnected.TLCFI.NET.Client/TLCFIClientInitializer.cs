﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using CodingConnected.JsonRPC;
using NLog;
using System.Linq;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Client.Helpers;
using CodingConnected.TLCFI.NET.Client.Session;
using CodingConnected.TLCFI.NET.Core.Data;
using CodingConnected.TLCFI.NET.Core.Exceptions;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.TLC;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using CodingConnected.TLCFI.NET.Core.Tools;
using CodingConnected.TLCFI.NET.Core.Extensions;

namespace CodingConnected.TLCFI.NET.Client
{
	public interface ITLCFIClientInitializer
	{
		Task InitializeSession(TLCFIClientSession session, TLCFIClientConfig config, TLCFIClientStateManager stateManager, CancellationToken token);
	}

	internal class TLCFIClientInitializer : ITLCFIClientInitializer
	{
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
		
        private ObjectReference _facilitiesRef;

		#endregion // Fields

		#region Properties
		
		#endregion // Properties

		#region Evenets

		[UsedImplicitly]
        public event EventHandler ApplicationRegistered;
        [UsedImplicitly]
        public event EventHandler ApplicationConfigured;

        #endregion // Evenets

        #region Public Methods

        public async Task InitializeSession(TLCFIClientSession session, TLCFIClientConfig config, TLCFIClientStateManager stateManager, CancellationToken token)
        {
	        if (config == null)
	        {
		        throw new NullReferenceException("Config is null, has SetConfig been called first?");
	        }
            try
            {
                var sessionId = await RegisterAsync(session, config, token);
                if (!session.State.Registered) throw new TLCFISessionException("Registering with TLC failed");
                ApplicationRegistered?.Invoke(this, EventArgs.Empty);
                session.StartAliveTimers();
                await GetSessionDataAsync(sessionId, session, config, stateManager, token);
                if (stateManager == null)
                {
                    return;
                }
                await ReadFacilitiesMetaAsync(session, config, stateManager, token);
                Intersection inter = null;
                if (!config.UseIdsFromTLCForSubscription)
                {
                    inter = await ReadIntersectionMetaAndSubscribeAsync(session, config, stateManager, token);
                }
                var refs = CollectAllRefs(stateManager.Facilities, inter, config);

                CheckMetaData(stateManager.Facilities, inter, config);
                await ReadAllObjectsMetaAsync(refs, session, config, stateManager, token);
                await SubscribeAllObjectsAsync(refs, session, stateManager, token);
                ApplicationConfigured?.Invoke(this, EventArgs.Empty);
                await SetInitialControlState(session, stateManager);
                _logger.Info("Client configured succesfully. Now ready to request control.");
            }
            catch (TLCFISessionException e)
            {
				_logger.Fatal(e, "Error initializing session. " + (e.Fatal ? "(FATAL!): " : ": "));
                throw new TLCFISessionException("Error initializing session. " + (e.Fatal ? "(FATAL!) " : ""), e.Fatal);
            }
        }

        #endregion // Public Methods

        #region Private Methods

        private async Task<string> RegisterAsync(TLCFIClientSession session, TLCFIClientConfig config, CancellationToken token)
        {
            _logger.Info("Registering with TLC.");
            try
            {
                // Register with TLC
                var rr = new RegistrationRequest
                {
                    Username = config.Username,
                    Password = config.Password,
                    Version = TLCFIDataProvider.Default.ProtocolVersion,
                    Type = ApplicationType.Control
                };
	            try
	            {
		            rr.Uri = !string.IsNullOrWhiteSpace(config.IveraUri)
			            ? new Uri(config.IveraUri)
			            : new Uri("http://10.0.0.0:12345");
	            }
	            catch (Exception e)
	            {
		            _logger.Error(e, "Error configuring IVERA uri; reverting to default [http://10.0.0.0:12345]. Exception: ");
	            }
                var reply = await session.TLCProxy.RegisterAsync(rr, token);

	            _facilitiesRef = reply?.Facilities ?? throw new RegistrationFailedException("Received null as a reply to RegisterAsync().");

                session.State.Registered = true;
                _logger.Info("Registered succesful.");
                return reply.Sessionid;
            }
            catch (JsonRpcException e)
            {
                session.State.Registered = false;
                _logger.LogRpcException(e);
                return null;
            }
            catch (Exception e)
            {
                session.State.Registered = false;
                _logger.Info(e, "Register failed with non-jsonrpc error: ");
                return null;
            }
        }

        private async Task<TLCFIClientStateManager> GetSessionDataAsync(string sessionId, TLCFIClientSession session, TLCFIClientConfig config, TLCFIClientStateManager stateManager, CancellationToken token)
        {
            _logger.Info("Obtaining session data and subscribing to session.");
            try
            {
                // Read meta for session
                var sref = new ObjectReference()
                {
                    Ids = new[] { sessionId },
                    Type = TLCObjectType.Session
                };
                var meta = await session.TLCProxy.ReadMetaAsync(sref, token);
                token.ThrowIfCancellationRequested();
                if (meta == null || meta.Meta.Length != 1)
                {
                    throw new InvalidMetaReceivedException("Incorrect response while reading session META. Meta received: " + meta);
                }

                // Check and store data, set state for session, and subscribe to session updates
                stateManager.Session = (TLCSessionBase) meta.Meta[0];
                ValueChecker.CheckValidObjectId(stateManager.Session.Id);
                switch (config.ApplicationType)
                {
                    case ApplicationType.Consumer:
                    case ApplicationType.Provider:
                        break;
                    case ApplicationType.Control:
	                    if (stateManager.Session is ControlApplication ct)
                        {
                            // Start with Error state: either the TLC will set it to Offline, or we will in SetInitialControlState
                            ct.ReqControlState = ControlState.Error; 
                            ct.StartCapability = config.StartCapability;
                            ct.EndCapability = config.EndCapability;
                            ct.ReqIntersection = config.RemoteIntersectionId;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (stateManager.Session.SessionType != config.ApplicationType)
                {
                    throw new InvalidTLCObjectTypeException($"Type of Session (ApplicationType) incorrect. Expected {config.ApplicationType.ToString()}, got {stateManager.Session.SessionType.ToString()}");
                }
                var data = await session.TLCProxy.SubscribeAsync(sref, token);
                token.ThrowIfCancellationRequested();
                if (data == null || data.Data.Length != 1)
                {
                    throw new InvalidStateReceivedException("Incorrect response while reading session STATE. State received: " + data);
                }

                ApplicationRegistered?.Invoke(this, EventArgs.Empty);

                _logger.Info("Succesfully got session meta and state: registered properly.");
                return stateManager;
            }
            catch (JsonRpcException e)
            {
                session.State.Registered = false;
                _logger.LogRpcException(e);
            }
            catch (Exception e)
            {
                session.State.Registered = false;
                _logger.Info(e, "Register failed with non-jsonrpc error.");
            }
            return null;
        }

        private async Task ReadFacilitiesMetaAsync(TLCFIClientSession session, TLCFIClientConfig config, TLCFIClientStateManager stateManager, CancellationToken token)
        {
            try
            {
                if (_facilitiesRef != null)
                {
                    ObjectMeta facilitiesMeta = null;
                    try
                    {
                        _logger.Info("Getting TLCFacilities META data");
                        facilitiesMeta = await session.TLCProxy.ReadMetaAsync(_facilitiesRef, token);
                    }
                    catch (JsonRpcException e)
                    {
                        _logger.LogRpcException(e);
                    }

                    if (facilitiesMeta != null && facilitiesMeta.Meta.Length == 1)
                    {
                        _logger.Info("Succesfully obtained TLCFacilities META data");
                        var facilitiesData = (TLCFacilities)facilitiesMeta.Meta[0];
                        stateManager.Facilities = facilitiesData;
                        if (!facilitiesData.Intersections.Contains(config.RemoteIntersectionId))
                        {
                            _logger.Error("Intersection with id {0} not found in TLCFacilities META data",
                                config.RemoteIntersectionId);
                            throw new TLCObjectNotFoundException(config.RemoteIntersectionId, TLCObjectType.Intersection);
                        }
                    }
                    else
                    {
                        _logger.Fatal("Error reading META of TLCFacilities: received {0} objects, expected 1",
                            facilitiesMeta?.Meta.Length ?? 0);
                        throw new ArgumentOutOfRangeException();
                    }
                }
                else
                {
                    _logger.Error(
                        "Error reading META of TLCFacilities: reference to facilities is null; was Register() succesfully called?");
                    throw new NullReferenceException();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error reading META of TLCFacilities, canceling session; ");
                throw new TLCFISessionException("Error reading META of TLCFacilities, canceling session");
            }
        }

        private async Task<Intersection> ReadIntersectionMetaAndSubscribeAsync(TLCFIClientSession session, TLCFIClientConfig config, TLCFIClientStateManager stateManager, CancellationToken token)
        {
            ObjectMeta intersectionMeta = null;
            ObjectData intersectionState = null;
            var iref = new ObjectReference
            {
                Ids = new[] { config.RemoteIntersectionId },
                Type = TLCObjectType.Intersection
            };

            try
            {
                _logger.Info("Getting Intersection META data for intersection with id {0}", config.RemoteIntersectionId);
                intersectionMeta = await session.TLCProxy.ReadMetaAsync(iref, token);
            }
            catch (JsonRpcException e)
            {
                _logger.LogRpcException(e);
            }

            if (intersectionMeta == null || intersectionMeta.Meta.Length != 1)
            {
                var exmes = $"Error reading META of Intersection: received {intersectionMeta?.Meta.Length ?? 0} objects, expected 1";
                _logger.Warn(exmes);
                throw new InvalidMetaReceivedException(exmes);
            }

            _logger.Info("Succesfully obtained Intersection META data");
            var intersectionData = (Intersection) intersectionMeta.Meta[0];
            stateManager.InternalIntersections.Add(intersectionData);

            try
            {
                intersectionState = await session.TLCProxy.SubscribeAsync(iref, token);
            }
            catch (JsonRpcException e)
            {
                _logger.LogRpcException(e);
            }

            if (intersectionState == null || intersectionState.Data.Length != 1)
            {
                var exmes = $"Error reading STATE of Intersection: received {intersectionState?.Data.Length ?? 0} objects, expected 1";
                _logger.Warn(exmes);
                throw new InvalidMetaReceivedException(exmes);
            }

            var ins = (Intersection)intersectionState.Data[0];
            var sins = stateManager.InternalIntersections.First(x => x.Id == intersectionState.Objects.Ids[0]);

            // Copy state
            sins.StateTicks = ins.StateTicks;
            sins.State = ins.State;
            session.State.IntersectionControl = sins.State == IntersectionControlState.Control;

            return intersectionData;
        }

        private List<ObjectReference> CollectAllRefs(TLCFacilities facilitiesData, Intersection intersectionData, TLCFIClientConfig config)
        {
            if (!config.UseIdsFromTLCForSubscription && intersectionData == null)
            {
                throw new NullReferenceException("IntersectionData may not be null when ids from intersection must be used to gather META data.");
            }
            if (config.UseIdsFromTLCForSubscription && facilitiesData == null)
            {
                throw new NullReferenceException("FacilitiesData may not be null when ids from TLCFacilities must be used to gather META data.");
            }
            var refs = new List<ObjectReference>
            {
                new ObjectReference
                {
                    Ids = config.UseIdsFromTLCForSubscription ? facilitiesData.Signalgroups : intersectionData.Signalgroups,
                    Type = TLCObjectType.SignalGroup
                },
                new ObjectReference
                {
                    Ids = config.UseIdsFromTLCForSubscription ? facilitiesData.Detectors : intersectionData.Detectors,
                    Type = TLCObjectType.Detector
                },
                new ObjectReference
                {
                    Ids = config.UseIdsFromTLCForSubscription || config.SubscribeToAllOutputs ? facilitiesData.Outputs : intersectionData.Outputs,
                    Type = TLCObjectType.Output
                },
                new ObjectReference
                {
                    Ids = config.UseIdsFromTLCForSubscription ? facilitiesData.Inputs : intersectionData.Inputs,
                    Type = TLCObjectType.Input
                },
                new ObjectReference
                {
                    Ids = config.UseIdsFromTLCForSubscription
                        ? new[] {facilitiesData.Spvehgenerator}
                        : new[] {intersectionData.Spvehgenerator},
                    Type = TLCObjectType.SpecialVehicleEventGenerator
                },
                new ObjectReference
                {
                    Ids = facilitiesData.Variables,
                    Type = TLCObjectType.Variable
                }
            };

            if (config.UseIdsFromTLCForSubscription)
            {
                refs.Add(new ObjectReference
                {
                    Ids = new[] { intersectionData.Id },
                    Type = TLCObjectType.Intersection
                });
            }
            return refs;
        }

        private void CheckMetaData(TLCFacilities facilitiesData, Intersection intersectionData,TLCFIClientConfig config)
        {
            bool ok;
            if (config.UseIdsFromTLCForSubscription)
            {
                _logger.Info("Checking CLA config against TLC META data.");
                ok = TLCFIClientCompatabilityChecker.IsCLACompatibleWithTLC(config, facilitiesData);
            }
            else
            {
                _logger.Info("Checking CLA config against Intersection META data.");
                ok = TLCFIClientCompatabilityChecker.IsCLACompatibleWithIntersection(config, intersectionData,
                    config.SubscribeToAllOutputs ? facilitiesData : null);
            }
            if (ok)
            {
                _logger.Info("All necessarry configured objects were found.");
                return;
            }
	        _logger.Fatal("Not all necessarry objects could be matched.");
            throw new TLCFISessionException("Not all necessarry object could be matched (FATAL!)", true);
        }

        private async Task ReadAllObjectsMetaAsync(IEnumerable<ObjectReference> refs, TLCFIClientSession session, TLCFIClientConfig config, TLCFIClientStateManager stateManager, CancellationToken token)
        {
            if (!session.State.Registered)
            {
                _logger.Warn(
                    "Error configuring application: not authorized with TLC; were Register() and ReadFacilitiesMeta() called?");
                throw new TLCFISessionException("Client is authorized with TLC; were Register() and ReadFacilitiesMeta() called?");
            }

            try
            {
                var getmetatasks = (from objref in refs
                        where objref.Ids.Length > 0
                        select session.TLCProxy.ReadMetaAsync(objref, token)).Cast<Task>()
                    .ToList();

                await Task.WhenAll(getmetatasks.ToArray());

                foreach (var t in getmetatasks)
                {
                    using (var task = t as Task<ObjectMeta>)
                    {
                        if (task == null) continue;

                        var meta = task.Result;

                        switch (meta.Objects.Type)
                        {
                            case TLCObjectType.SignalGroup:
                                foreach (SignalGroup sg in meta.Meta)
                                {
                                    stateManager.InternalSignalGroups.Add(sg);
                                }
                                break;
                            case TLCObjectType.Detector:
                                foreach (Detector d in meta.Meta)
                                {
                                    stateManager.InternalDetectors.Add(d);
                                }
                                break;
                            case TLCObjectType.Input:
                                foreach (Input ip in meta.Meta)
                                {
                                    stateManager.InternalInputs.Add(ip);
                                }
                                break;
                            case TLCObjectType.Output:
                                foreach (Output op in meta.Meta)
                                {
                                    stateManager.InternalOutputs.Add(op);
                                }
                                break;
                            case TLCObjectType.SpecialVehicleEventGenerator:
                                stateManager.SpvhGenerator = (SpecialVehicleEventGenerator)meta.Meta[0];
                                break;
                            case TLCObjectType.Variable:
                                foreach (Variable op in meta.Meta)
                                {
                                    stateManager.InternalVariables.Add(op);
                                }
                                break;
                            case TLCObjectType.Session: // Special kind of object; gets initialized in Register()
                            case TLCObjectType.TLCFacilities: // Got initialized in ReadFacilitiesMeta()
                            case TLCObjectType.Intersection: // Got initialized in ReadFacilitiesMeta()
                                throw new NotSupportedException();
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            catch (JsonRpcException e)
            {
                _logger.LogRpcException(e);
                throw new TLCFISessionException("Error reading META from TLCFacilities.");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while reading META data for all objects from TLC");
                throw new TLCFISessionException("Error reading META from TLCFacilities.");
            }

	        try
	        {
		        stateManager.Initialize(config.RemoteIntersectionId); // Check and init
	        }
	        catch (DuplicateNameException)
            {
                throw new TLCFISessionException("Error checking META data from TLCFacilities (FATAL!)", true);
            }
        }

        private async Task SubscribeAllObjectsAsync(IEnumerable<ObjectReference> refs, TLCFIClientSession session, TLCFIClientStateManager stateManager, CancellationToken token)
        {
            try
            {
                var getstatetasks = (from objref in refs
                        where objref.Ids.Length > 0
                        select session.TLCProxy.SubscribeAsync(objref, token)).Cast<Task>()
                    .ToList();

                await Task.WhenAll(getstatetasks.ToArray());

                foreach (var t in getstatetasks)
                {
                    using (var task = t as Task<ObjectData>)
                    {
                        if (task == null) continue;

                        var data = task.Result;

                        switch (data.Objects.Type)
                        {
                            case TLCObjectType.SignalGroup:
                                for (var i = 0; i < data.Data.Length; ++i)
                                {
                                    var sg = (SignalGroup)data.Data[i];
                                    var ssg = stateManager.InternalSignalGroups.First(x => x.Id == data.Objects.Ids[i]);
                                    if (ssg == null)
                                    {
                                        throw new NullReferenceException();
                                    }
                                    // copy state
                                    ssg.StateTicks = sg.StateTicks;
                                    ssg.State = sg.State;
                                    ssg.Predictions = sg.Predictions;
                                }
                                break;
                            case TLCObjectType.Detector:
                                for (var i = 0; i < data.Data.Length; ++i)
                                {
                                    var d = (Detector)data.Data[i];
                                    var sd = stateManager.InternalDetectors.First(x => x.Id == data.Objects.Ids[i]);
                                    if (sd == null)
                                    {
										throw new NullReferenceException();
									}
                                    // copy state
                                    sd.StateTicks = d.StateTicks;
                                    sd.State = d.State;
                                    sd.FaultState = d.FaultState;
                                    sd.Swico = d.Swico;
                                }
                                break;
                            case TLCObjectType.Input:
                                for (var i = 0; i < data.Data.Length; ++i)
                                {
                                    var ip = (Input)data.Data[i];
                                    var sip = stateManager.InternalInputs.First(x => x.Id == data.Objects.Ids[i]);
                                    if (sip == null)
                                    {
										throw new NullReferenceException();
									}
                                    // copy state
                                    sip.StateTicks = ip.StateTicks;
                                    sip.State = ip.State;
                                    sip.FaultState = ip.FaultState;
                                    sip.Swico = ip.Swico;
                                }
                                break;
                            case TLCObjectType.Output:
                                for (var i = 0; i < data.Data.Length; ++i)
                                {
                                    var op = (Output)data.Data[i];
                                    var sop = stateManager.InternalOutputs.First(x => x.Id == data.Objects.Ids[i]);
                                    if (sop == null)
                                    {
										throw new NullReferenceException();
									}
                                    // copy state
                                    sop.StateTicks = op.StateTicks;
                                    sop.State = op.State;
                                    sop.FaultState = op.FaultState;
                                }
                                break;
                            case TLCObjectType.Intersection:
                                for (var i = 0; i < data.Data.Length; ++i)
                                {
                                    var ins = (Intersection)data.Data[i];
                                    var sins = stateManager.InternalIntersections.First(x => x.Id == data.Objects.Ids[i]);
                                    if (sins == null)
                                    {
										throw new NullReferenceException();
									}
                                    // copy state
                                    sins.StateTicks = ins.StateTicks;
                                    sins.State = ins.State;
                                    session.State.IntersectionControl = sins.State == IntersectionControlState.Control;
                                }
                                break;
                            case TLCObjectType.SpecialVehicleEventGenerator:
                                if (data.Data.Length == 1)
                                {
                                    var spv = (SpecialVehicleEventGenerator)data.Data[0];
                                    var sspv = stateManager.SpvhGenerator;
                                    if (spv == null)
                                    {
										throw new NullReferenceException();
									}
                                    // copy state
                                    sspv.FaultState = spv.FaultState;
                                }
                                break;
                            case TLCObjectType.Variable:
                                for (var i = 0; i < data.Data.Length; ++i)
                                {
                                    var v = (Variable)data.Data[i];
                                    var sv = stateManager.InternalVariables.First(x => x.Id == data.Objects.Ids[i]);
                                    if (sv == null)
                                    {
										throw new NullReferenceException();
									}
                                    // copy state
                                    sv.Lifetime = v.Lifetime;
                                    sv.Value = v.Value;
                                }
                                break;
                            case TLCObjectType.Session:         // Already subscribed in Register()
                            case TLCObjectType.TLCFacilities:   // This object does not have a state
								throw new NotSupportedException();
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                _logger.Info("Succesfully initialized TLC state in CLA");
            }
            catch (JsonRpcException e)
            {
                _logger.LogRpcException(e);
                throw new TLCFISessionException("Error while subscribing to objects in TLCFacilities.");
            }
            catch (Exception e)
            {
                _logger.Error(e, "Error while obtaining objects from TLC");
                throw new TLCFISessionException("Error while subscribing to objects in TLCFacilities.");
            }
        }

        private async Task SetInitialControlState(TLCFIClientSession session, TLCFIClientStateManager stateManager)
        {
            _logger.Info("Setting initial application session state in TLC (with ControlState.Offline) if needed");
            try
            {
                if(stateManager.ControlSession.ReqControlState != ControlState.Offline)
                    await session.SetReqControlStateAsync(ControlState.Offline);
                session.State.Configured = true;
                _logger.Debug("Succesfully set initial application session state in TLC");
            }
            catch (JsonRpcException e)
            {
                _logger.LogRpcException(e);
                throw new TLCFISessionException("Error while setting initial application control state in TLCFacilities.");
            }
        }

        #endregion // Private Methods

        #region Constructor

        public TLCFIClientInitializer()
        {
        }

        #endregion // Constructor
    }
}