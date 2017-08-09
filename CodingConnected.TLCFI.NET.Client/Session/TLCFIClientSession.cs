using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using CodingConnected.JsonRPC;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Data;
using CodingConnected.TLCFI.NET.Generic;
using CodingConnected.TLCFI.NET.Helpers;
using CodingConnected.TLCFI.NET.Models.Generic;
using CodingConnected.TLCFI.NET.Models.TLC;
using CodingConnected.TLCFI.NET.Proxies;
using JetBrains.Annotations;
using NLog;
using Timer = System.Timers.Timer;

namespace CodingConnected.TLCFI.NET.Client.Session
{
    /// <summary>
    /// Represents the session with a remote TLC Facilities.
    /// This class handled TLC-FI JSON-RPC calls and monitors session health via alive checking.
    /// The class responds to JSON-RPC calls by calling methods and setting state in the CLABase class.
    /// </summary>
    internal class TLCFIClientSession
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IPEndPoint _endPoint;
        private readonly Timer _aliveSendTimer = new Timer(TLCFIDataProvider.Default.Settings.AliveSendTimeOut);
        private readonly Timer _aliveReceivedTimer = new Timer(TLCFIDataProvider.Default.Settings.AliveReceiveTimeOut);
        private readonly CancellationToken _sessionCancellationToken;
        private readonly TLCFIClientSessionJsonRpcHandler _jsonRpcHandler;
        private readonly TLCFIClientStateManager _stateManager;
        private int _aliveSendFailCounter;

        #endregion // Fields

        #region Properties

        private TwoWayTcpClient Client { get; }
        internal TLCProxy TLCProxy { get; }

        public IPEndPoint RemoteEndPoint { get; }
        [UsedImplicitly]
        public IPEndPoint LocalEndPoint => Client?.LocalEndPoint;
        public bool Connected => Client?.Connected ?? false;
        public TLCFIClientSessionState State { get; }

        #endregion // Properties

        #region Events

        [UsedImplicitly]
        public event EventHandler<string> DataReceived;
        [UsedImplicitly]
        public event EventHandler<string> DataSent;

        public event EventHandler ReceiveAliveTimeoutOccured;
        public event EventHandler SendAliveTimeoutOccured;
        public event EventHandler Disconnected;
        public event EventHandler<bool> SessionEnded;
        public event EventHandler ControlStateSetToError;

        #endregion // Events

        #region Public Methods

        public async Task StartSessionAsync(int timeout)
        {
            try
            {
                await Client.ConnectAsync(_endPoint, timeout, _sessionCancellationToken);
            }
            catch (TaskCanceledException)
            {

            }
            _aliveSendTimer.AutoReset = true;
            _aliveSendTimer.Enabled = false;
            _aliveSendTimer.Elapsed += AliveSendTimer_Elapsed;
            _aliveReceivedTimer.Elapsed += AliveReceivedTimer_Elapsed;
        }

        public void StartAliveTimers()
        {
            _aliveSendTimer.Start();
            _aliveReceivedTimer.Start();
        }

        public void DisposeSession()
        {
            _aliveSendTimer.Dispose();
            _aliveReceivedTimer.Dispose();
            TLCProxy?.Dispose();
            Client?.Dispose();
        }

        public async Task CloseSessionAsync(bool expected)
        {
            try
            {
                _logger.Info("Deregistered succesfully from TLC.");
                if (Connected && State.Controlling)
                {
                    var maxdl = Task.Delay(TLCFIDataProvider.Default.Settings.MaxReleaseControlDuration,
                        _sessionCancellationToken);
                    var relct = Task.Run(async () =>
                    {
                        while (State.Controlling)
                        {
                            await Task.Delay(100, _sessionCancellationToken);
                        }
                    }, _sessionCancellationToken);
                    await Task.WhenAny(maxdl, relct);
                }
                StopAliveTimers();
                if (Connected && State.Registered)
                {
                    try
                    {
                        await TLCProxy.DeregisterAsync(new DeregistrationRequest(),
                            _sessionCancellationToken);
                        _logger.Info("Deregistered succesfully from TLC.");
                    }
                    catch
                    {
                        // ignore
                    }
                }
                SessionEnded?.Invoke(this, expected);
            }
            catch (TaskCanceledException)
            {
                
            }
        }

        public async Task SetReqControlStateAsync(ControlState state)
        {
            if (_stateManager.ControlSession.ReqControlState != state)
            {
                await _jsonRpcHandler.SetReqControlStateAsync(state);
            }
        }

        public async Task SetIntersectionReqStateAsync(IntersectionControlState state)
        {
            await _jsonRpcHandler.SetIntersectionReqStateAsync(state);
        }

        #endregion // Public Methods

        #region Private Methods

        private void StopAliveTimers()
        {
            _aliveSendTimer.Stop();
            _aliveReceivedTimer.Stop();
        }

        private async void AliveSendTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if(!Connected || !State.SystemsAlive)
                    return;

                var ao = new AliveObject {Ticks = TLCFIClient.CurrentTicks, Time = TLCFIClient.CurrentTime};
                var reply = await TLCProxy.AliveAsync(ao, _sessionCancellationToken);
                if (reply != null && reply.Ticks == ao.Ticks && reply.Time == ao.Time)
                {
                    return;
                }

                _aliveSendFailCounter++;
                if (_aliveSendFailCounter > 2)
                {
                    StopAliveTimers();
                    SendAliveTimeoutOccured?.Invoke(this, EventArgs.Empty);
                }
            }
            catch
            {
                // ignored
            }
        }

        private void AliveReceivedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _logger.Warn("Receive Alive timeout occured");
            State.Registered = false;
            StopAliveTimers();
            ReceiveAliveTimeoutOccured?.Invoke(this, EventArgs.Empty);
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            StopAliveTimers();
            Disconnected?.Invoke(this, EventArgs.Empty);
            SessionEnded?.Invoke(this, false);
        }

        private void OnUpdateStateCalled(object sender, ObjectStateUpdate objectstateupdate)
        {
            switch (objectstateupdate.Objects.Type)
            {
                case TLCObjectType.Session:
                    if (objectstateupdate.Objects.Ids.Length == 1 && objectstateupdate.Objects.Ids[0] == _stateManager.Session.Id &&
                        objectstateupdate.States.Length == 1)
                    {
                        var application = (ControlApplication)objectstateupdate.States[0];
                        if (application == null)
                        {
                            throw new JsonRpcException((int)ProtocolErrorCode.InvalidObjectReference,
                                "State could not be properly cast to type ControlApplication (object type: TLCObjectType.Session).", null);
                        }
                        if (!application.ControlState.HasValue)
                        {
                            throw new JsonRpcException((int)ProtocolErrorCode.InvalidAttributeValue,
                                "ControlState was not set on ControlApplication object.", null);
                        }
                        _logger.Info("TLC set Application.ControlState from {0} to {1}.",
                            _stateManager.ControlSession.ControlState, application.ControlState);
                        if (_stateManager.ControlSession.ControlState.HasValue && application.ControlState.HasValue)
                            if (!TLCFIStateChecker.IsControlStateChangeOk(_stateManager.ControlSession.ControlState.Value,
                                application.ControlState.Value))
                            {
                                _logger.Warn("Invalid ControlState transition made by TLC: from {0} to {1}. Resulting behaviour is undefined.",
                                    _stateManager.ControlSession.ControlState, application.ControlState);
                            }

                        switch (application.ControlState)
                        {
                            case ControlState.Error:
                                State.SessionControl = false;
                                _logger.Error("TLC set Application.ControlState to ControlState.Error.");
                                ControlStateSetToError?.Invoke(this, EventArgs.Empty);
                                break;

                            case ControlState.NotConfigured:
                                State.SessionControl = false;
                                // During startup, accept transition from 0 (error) to NotConfigured
                                if (_stateManager.ControlSession.ReqControlState == ControlState.Error)
                                {
                                    _logger.Info("Will confirm ControlState by setting requested state to ControlState.NotConfigured.");
                                    Task.Run(() => SetReqControlStateAsync(ControlState.NotConfigured), _sessionCancellationToken);
                                }
                                // Otherwise, if not requested, this transition is false
                                else if (_stateManager.ControlSession.ReqControlState != ControlState.NotConfigured)
                                {
                                    _logger.Warn("TLC set Application.ControlState to ControlState.NotConfigured. " +
                                                 "(Requested = {0}).", _stateManager.ControlSession.ReqControlState);
                                }
                                break;
                            case ControlState.Offline:
                                State.SessionControl = false;
                                // Accept Offline during startup
                                if ((_stateManager.ControlSession.ReqControlState == ControlState.Error ||
                                     _stateManager.ControlSession.ReqControlState == ControlState.NotConfigured) && 
                                    _stateManager.ControlSession.ControlState == ControlState.Offline)
                                {
                                    _logger.Info("Will confirm ControlState by setting requested state from ControlState.NotConfigured to ControlState.Offline.");
                                    Task.Run(() => SetReqControlStateAsync(ControlState.Offline), _sessionCancellationToken);
                                }
                                // Log Offline as false otherwise: the CLA should have requested this first
                                else if (_stateManager.ControlSession.ReqControlState != ControlState.Offline)
                                {
                                    _logger.Warn(
                                        "TLC set Application.ControlState to ControlState.Offline. (Requested = {0}).",
                                        _stateManager.ControlSession.ReqControlState);
                                }
                                // Otherwise Offline was requested
                                else
                                {
                                    _logger.Info("TLC set Application.ControlState to ControlState.Offline. Now awaiting instruction to request control.");
                                }
                                break;
                            case ControlState.ReadyToControl:
                                State.SessionControl = false;
                                // Log if not as requested
                                if (_stateManager.ControlSession.ReqControlState != ControlState.ReadyToControl)
                                {
                                    _logger.Error("TLC set Application.ControlState to ControlState.ReadyToControl, while requested state is {0}.", _stateManager.ControlSession.ReqControlState);
                                }
                                // Otherwise log awaiting StartControl
                                else
                                {
                                    _logger.Info("TLC set Application.ControlState to ControlState.ReadyToControl. Now awaiting StartControl.");
                                }
                                break;
                            case ControlState.InControl:
                                // Log if not as requested
                                if (_stateManager.ControlSession.ReqControlState != ControlState.InControl)
                                {
                                    _logger.Error("TLC set Application.ControlState to ControlState.InControl. (Requested = {0}).", _stateManager.ControlSession.ReqControlState);
                                }
                                // Otherwise log confirmation
                                else
                                {
                                    _logger.Info("TLC set Application.ControlState to ControlState.InControl.");
                                }
                                break;
                            case ControlState.StartControl:
                                // If we requested control, take action to actually take it
                                if (_stateManager.ControlSession.ReqControlState == ControlState.ReadyToControl)
                                {
                                    _logger.Info("Application.ControlState set to ControlState.StartControl. (Requested = {0}).", _stateManager.ControlSession.ReqControlState);
                                    State.SessionControl = true;
                                    Task.Run(() => SetReqControlStateAsync(ControlState.InControl), _sessionCancellationToken);
                                }
                                // Otherwise, log the error
                                else
                                {
                                    var startControlError = "Application.ControlState set to ControlState.StartControl, but application not ready.";
                                    _logger.Error(startControlError);
                                    throw new JsonRpcException((int)ProtocolErrorCode.Error, startControlError, null);
                                }
                                break;
                            case ControlState.EndControl:
                                // If the application is not in Control, log the error
                                if (_stateManager.ControlSession.ReqControlState != ControlState.InControl &&
                                    _stateManager.ControlSession.ReqControlState != ControlState.EndControl)
                                {
                                    _logger.Error("TLC set Application.ControlState to ControlState.EndControl. (Requested = {0}).", _stateManager.ControlSession.ReqControlState);
                                    throw new JsonRpcException((int)ProtocolErrorCode.Error, "TLC set Application.ControlState to ControlState.EndControl, but application not in control.", null);
                                }
                                else
                                {
                                    if (_stateManager.ControlSession.ReqControlState == ControlState.EndControl)
                                    {
                                        _logger.Info("TLC set Application.ControlState to ControlState.EndControl, which was requested.");
                                    }
                                    else
                                    {
                                        _logger.Info("TLC set Application.ControlState to ControlState.EndControl (outside request). " +
                                                     "Confirming by setting requested state.");
                                        Task.Run(() => SetReqControlStateAsync(ControlState.EndControl), _sessionCancellationToken);
                                    }
                                }
                                break;

                            default:
                                var error = $"Application.ControlState cannot be set to {application.ControlState}: this state is undefined.";
                                _logger.Error(error);
                                throw new JsonRpcException((int)ProtocolErrorCode.Error, error, null);
                        }
                        _stateManager.ControlSession.ControlState = application.ControlState;
                        if (_stateManager.ControlSession.ControlState.Value != ControlState.Error)
                        {
                            _logger.Debug("Application.ControlState set to " + _stateManager.ControlSession.ControlState);
                        }
                    }
                    else
                    {
                        var error = $"UpdateState called with type Session, but {objectstateupdate.Objects.Ids.Length} instead of 1 object provided.";
                        _logger.Error(error);
                        throw new JsonRpcException((int)ProtocolErrorCode.Error, error, null);
                    }
                    break;
                case TLCObjectType.Intersection:
                    for (var i = 0; i < objectstateupdate.Objects.Ids.Length; ++i)
                    {
                        var inter = _stateManager.InternalIntersections.FirstOrDefault(x => x.Id == objectstateupdate.Objects.Ids[i]);
                        if (inter == null)
                        {
                            throw new JsonRpcException((int)ProtocolErrorCode.InvalidObjectReference, "Object " + objectstateupdate.Objects.Ids[i] + " unknown", null);
                        }
                        var sinter = (Intersection)objectstateupdate.States[i];
                        inter.StateTicks = sinter.StateTicks;
                        if (sinter.State.HasValue) inter.State = sinter.State;
                        if (sinter.ReqState.HasValue) inter.ReqState = sinter.ReqState;
                        switch (sinter.State)
                        {
                            case IntersectionControlState.Error:
                            case IntersectionControlState.Dark:
                            case IntersectionControlState.Standby:
                            case IntersectionControlState.AlternativeStandby:
                            case IntersectionControlState.AllRed:
                            case IntersectionControlState.SwitchOn:
                            case IntersectionControlState.SwitchOff:
                                State.IntersectionControl = false;
                                break;
                            case IntersectionControlState.Control:
                                State.IntersectionControl = true;
                                break;
                            default:
                                var error = $"Intersection.State cannot be set to {sinter.State}: This state is undefined.";
                                _logger.Error(error);
                                throw new JsonRpcException((int)ProtocolErrorCode.Error, error, null);
                        }
                        _logger.Debug("Intersection {0} state set to {1}", objectstateupdate.Objects.Ids[i], sinter.State);
                    }
                    break;
                case TLCObjectType.SignalGroup:
                case TLCObjectType.Detector:
                case TLCObjectType.Output:
                case TLCObjectType.Input:
                case TLCObjectType.Variable:
                    for (var i = 0; i < objectstateupdate.Objects.Ids.Length; ++i)
                    {
                        var upob = _stateManager.FindObjectById(objectstateupdate.Objects.Ids[i], TLCFIClientStateManager.GetObjectTypeString(objectstateupdate.Objects.Type));
                        if (upob == null)
                        {
                            throw new JsonRpcException((int)ProtocolErrorCode.InvalidObjectReference, "Object " + objectstateupdate.Objects.Ids[i] + " unknown", null);
                        }
                        try
                        {
                            ((TLCObjectBase) upob).CopyState(objectstateupdate.States[i]);
                        }
                        catch (InvalidCastException e)
                        {
                            throw new JsonRpcException((int)ProtocolErrorCode.InvalidObjectReference, "Object " + objectstateupdate.Objects.Ids[i] + " seems not to be of type " + objectstateupdate.Objects.Type, e);
                        }
                    }
                    break;
                case TLCObjectType.SpecialVehicleEventGenerator:
                    if (objectstateupdate.Objects.Ids.Length == 1 && objectstateupdate.Objects.Ids[0] == _stateManager.ControlSession.Id &&
                        objectstateupdate.States.Length == 1)
                    {
                        _stateManager.SpvhGenerator.CopyState(objectstateupdate.States[0]);
                    }
                    else
                    {
                        var error = $"UpdateState called with type SpecialVehicleEventGenerator, but {objectstateupdate.Objects.Ids.Length} instead of 1 object provided.";
                        _logger.Error(error);
                        throw new JsonRpcException((int)ProtocolErrorCode.Error, error, null);
                    }
                    break;
                case TLCObjectType.TLCFacilities:
                    var tlcferror = "UpdateState called with type TLCFacilities, which has no state.";
                    _logger.Error(tlcferror);
                    throw new JsonRpcException((int)ProtocolErrorCode.Error, tlcferror, null);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion // Private Methods

        #region Constructor

        public TLCFIClientSession(TLCFIClientStateManager stateManager, IPEndPoint ep, CancellationToken token)
        {
            _endPoint = ep;
            _sessionCancellationToken = token;
            _stateManager = stateManager;

            Client = new TwoWayTcpClient();
            Client.DataSent += (o, e) => { DataSent?.Invoke(this, e); };
            Client.Disconnected += Client_Disconnected;

            RemoteEndPoint = ep;

            State = new TLCFIClientSessionState();

            TLCProxy = new TLCProxy(Client);
            _jsonRpcHandler = new TLCFIClientSessionJsonRpcHandler(stateManager, TLCProxy, State, Client, token);
            _jsonRpcHandler.ReceivedAlive += (o, a) =>
            {
                _aliveReceivedTimer.Stop();
                _aliveReceivedTimer.Start();
            };
            _jsonRpcHandler.UpdateStateCalled += (o, e) => { OnUpdateStateCalled(this, e); };
        }

        #endregion // Constructor
    }
}
