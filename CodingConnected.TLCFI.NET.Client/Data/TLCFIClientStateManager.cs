﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;
using CodingConnected.JsonRPC;
using CodingConnected.TLCFI.NET.Exceptions;
using CodingConnected.TLCFI.NET.Models.Generic;
using CodingConnected.TLCFI.NET.Models.TLC;
using CodingConnected.TLCFI.NET.Models.TLC.Base;
using NLog;

namespace CodingConnected.TLCFI.NET.Client.Data
{
    public class TLCFIClientStateManager
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<string, object> _staticObjects = new Dictionary<string, object>();
        private readonly List<string> _changedObjects = new List<string>();

        private ReadOnlyDictionary<string, object> StaticObjects { get; set; }
        private Dictionary<string, object> DynamicObjects { get; set; }

        private readonly object _locker = new object();

        #endregion // Fields

        #region Internal Properties

        internal Intersection Intersection { get; private set; }
        internal TLCSessionBase Session { get; set; }
        internal ControlApplication ControlSession => Session as ControlApplication;

        internal List<Intersection> InternalIntersections { get; }
        internal List<Variable> InternalVariables { get; }
        internal List<SignalGroup> InternalSignalGroups { get; }
        internal List<Detector> InternalDetectors { get; }
        internal List<Input> InternalInputs { get; }
        internal List<Output> InternalOutputs { get; }
        
        #endregion // Internal Properties

        #region Public Properties

        public TLCFacilities Facilities { get; internal set; }
        public SpecialVehicleEventGenerator SpvhGenerator { get; internal set; }

        [UsedImplicitly]
        public ReadOnlyCollection<Intersection> Intersections { get; private set; }
        [UsedImplicitly]
        public ReadOnlyCollection<Variable> Variables { get; private set; }
        [UsedImplicitly]
        public ReadOnlyCollection<SignalGroup> SignalGroups { get; private set; }
        [UsedImplicitly]
        public ReadOnlyCollection<Detector> Detectors { get; private set; }
        [UsedImplicitly]
        public ReadOnlyCollection<Input> Inputs { get; private set; }
        [UsedImplicitly]
        public ReadOnlyCollection<Output> Outputs { get; private set; }

        #endregion // Public Properties

        #region Events

        internal event EventHandler<List<string>> StateChanged;
        internal event EventHandler<Detector> DetectorStateChanged;
        internal event EventHandler<SignalGroup> SignalGroupStateChanged;
        internal event EventHandler<Input> InputStateChanged;
        internal event EventHandler<Output> OutputStateChanged;

        #endregion // Events

        #region Internal Methods

        internal void RaiseStateChanged()
        {
            lock(_locker)
            {
                if (_changedObjects.Any())
                {
                    StateChanged?.Invoke(this, _changedObjects);
                }
            }
        }

        internal void ResetStateChanged()
        {
            lock (_locker)
            {
                _changedObjects.Clear();
            }
        }

        internal void Initialize(string intersectionId)
        {
            // Check unicity
            var ids = new List<List<TLCObjectBase>>
            {
                new List<TLCObjectBase>(InternalSignalGroups),
                new List<TLCObjectBase>(InternalDetectors),
                new List<TLCObjectBase>(InternalInputs),
                new List<TLCObjectBase>(InternalOutputs),
                new List<TLCObjectBase>(InternalVariables)
            };
            if (InternalIntersections.Count == 0)
            {
                throw new TLCFISessionException("No intersections are present in the collected data; cannot initialize StateManager.");
            }
            foreach (var l in ids)
            {
                foreach (var item1 in l)
                {
                    foreach (var item2 in l)
                    {
                        if (item1 != item2 && item1.Id == item2.Id)
                        {
                            throw new DuplicateNameException($"Found duplicate IDs: type {item1.ObjectType}, id {item1.Id}. " +
                                                             "All Ids in TLC config must be unique per type.");
                        }
                    }
                }
            }

            // Build complete list
            if (Facilities != null) _staticObjects.Add("_f_" + Facilities.Id, Facilities);
            if (SpvhGenerator != null) _staticObjects.Add("_sp_" + SpvhGenerator.Id, SpvhGenerator);
            foreach (var sg in InternalSignalGroups)
            {
                sg.ChangedState += (o, e) => { SignalGroupStateChanged?.Invoke(this, sg); };
                _staticObjects.Add("_sg_" + sg.Id, sg);
            }
            foreach (var d in InternalDetectors)
            {
                d.ChangedState += (o, e) => { DetectorStateChanged?.Invoke(this, d); };
                _staticObjects.Add("_d_" + d.Id, d);
            }
            foreach (var i in InternalInputs)
            {
                i.ChangedState += (o, e) => { InputStateChanged?.Invoke(this, i); };
                _staticObjects.Add("_i_" + i.Id, i);
            }
            foreach (var o in InternalOutputs)
            {
                o.ChangedState += (o2, e) => { OutputStateChanged?.Invoke(this, o); };
                _staticObjects.Add("_o_" + o.Id, o);

                // Set exclusive: if an output belongs to an intersection, it is exclusive
                if (InternalIntersections.SelectMany(x => x.Outputs).Any(x => x == o.Id))
                {
                    o.Exclusive = true;
                }
            }
            foreach (var v in InternalVariables)
            {
                _staticObjects.Add("_v_" + v.Id, v);
                // TODO: state change
            }
            foreach (var i in InternalIntersections)
            {
                _staticObjects.Add("_int_" + i.Id, i);
                if (i.Id == intersectionId)
                {
                    Intersection = i;
                }
                // TODO: state change
            }

            // Initialize properties
            StaticObjects = new ReadOnlyDictionary<string, object>(_staticObjects);
            DynamicObjects = new Dictionary<string, object>();
            Intersections = new ReadOnlyCollection<Intersection>(InternalIntersections);
            SignalGroups = new ReadOnlyCollection<SignalGroup>(InternalSignalGroups);
            Detectors = new ReadOnlyCollection<Detector>(InternalDetectors);
            Inputs = new ReadOnlyCollection<Input>(InternalInputs);
            Outputs = new ReadOnlyCollection<Output>(InternalOutputs);
            Variables = new ReadOnlyCollection<Variable>(InternalVariables);

            _logger.Info("Initializing data from remote TLC completed. TLC has:");
            _logger.Info("  - {0} intersections", InternalIntersections.Count);
            _logger.Info("  - {0} signalgroups", InternalSignalGroups.Count);
            _logger.Info("  - {0} detectors", InternalDetectors.Count);
            _logger.Info("  - {0} outputs", InternalOutputs.Count);
            _logger.Info("  - {0} inputs", InternalInputs.Count);
            _logger.Info("  - {0} variables", InternalVariables.Count);
            _logger.Info("  - {0} spvehiclegens", SpvhGenerator == null ? 0 : 1);
            foreach (var i in InternalIntersections)
            {
                _logger.Info("  Intersection {0} has:", i.Id);
                _logger.Info("    - {0} signalgroups", i.Signalgroups.Length);
                _logger.Info("    - {0} detectors", i.Detectors.Length);
                _logger.Info("    - {0} outputs", i.Outputs.Length);
                _logger.Info("    - {0} inputs", i.Inputs.Length);
                _logger.Info("    - {0} spvehiclegens", i.Spvehgenerator == null ? 0 : 1);
            }
        }

        internal object FindObjectById(string id)
        {
            if (Facilities.Id == id)
            {
                return Facilities;
            }

            if (SpvhGenerator.Id == id)
            {
                return SpvhGenerator;
            }

            object ret = InternalIntersections.FirstOrDefault(x => x.Id == id);
            if (ret != null)
            {
                return ret;
            }

            ret = InternalVariables.FirstOrDefault(x => x.Id == id);
            if (ret != null)
            {
                return ret;
            }

            ret = InternalSignalGroups.FirstOrDefault(x => x.Id == id);
            if (ret != null)
            {
                return ret;
            }

            ret = InternalDetectors.FirstOrDefault(x => x.Id == id);
            if (ret != null)
            {
                return ret;
            }

            ret = InternalInputs.FirstOrDefault(x => x.Id == id);
            if (ret != null)
            {
                return ret;
            }

            ret = InternalOutputs.FirstOrDefault(x => x.Id == id);
            if (ret != null)
            {
                return ret;
            }

            return null;
        }

        internal ObjectMeta GetObjectMeta(ObjectReference objectreference, uint ticks)
        {
            // Compile a list of all requested objects
            var objects = new List<object>();
            foreach (var id in objectreference.Ids)
            {
                var pfid = GetTypePrefix(objectreference.Type) + id;
                if (!StaticObjects.TryGetValue(pfid, out object ob))
                {
                    if (!DynamicObjects.TryGetValue(pfid, out ob))
                    {
                        throw new JsonRpcException(
                            (int)ProtocolErrorCode.InvalidObjectReference,
                            "Object with type " + objectreference.Type + " and id " + id + " unknown",
                            null);
                    }
                }
                objects.Add(ob);
            }

            // Build reply object
            var meta = new ObjectMeta
            {
                Objects = new ObjectReference()
                {
                    Ids = objects.Select(x => ((TLCObjectBase)x).Id).ToArray(),
                    Type = objectreference.Type
                },
                Ticks = ticks,
                Meta = objects.Select(x => ((TLCObjectBase)x).GetMeta()).ToArray()
            };

            return meta;
        }

        internal ObjectData GetObjectState(ObjectReference objectreference, uint ticks)
        {
            // Compile a list of all requested objects
            var objects = new List<object>();
            foreach (var id in objectreference.Ids)
            {
                var pfid = GetTypePrefix(objectreference.Type) + id;
                if (!StaticObjects.TryGetValue(pfid, out object ob))
                {
                    if (!DynamicObjects.TryGetValue(pfid, out ob))
                    {
                        throw new JsonRpcException(
                            (int)ProtocolErrorCode.InvalidObjectReference,
                            "Object with type " + objectreference.Type + " and id " + id + " unknown",
                            null);
                    }
                }
                objects.Add(ob);
            }

            // Build reply object
            var meta = new ObjectData
            {
                Objects = new ObjectReference
                {
                    Ids = objects.Select(x => ((TLCObjectBase)x).Id).ToArray(),
                    Type = objectreference.Type
                },
                Ticks = ticks,
                Data = objects.Select(x => ((TLCObjectBase)x).GetState(true)).ToArray()
            };

            return meta;
        }

        internal void SetObjectStateChanged(string id)
        {
            _changedObjects.Add(id);
        }

        #endregion // Internal Methods

        #region Private Methods

        private string GetTypePrefix(TLCObjectType type)
        {
            switch (type)
            {
                case TLCObjectType.TLCFacilities:
                    return "_f_";
                case TLCObjectType.Intersection:
                    return "_int_";
                case TLCObjectType.SignalGroup:
                    return "_sg_";
                case TLCObjectType.Detector:
                    return "_d_";
                case TLCObjectType.Input:
                    return "_i_";
                case TLCObjectType.Output:
                    return "_o_";
                case TLCObjectType.SpecialVehicleEventGenerator:
                    return "_sp_";
                case TLCObjectType.Variable:
                    return "_v_";
                case TLCObjectType.Session:
                    return "_ses_";
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion // Private Methods

        #region Constructor

        public TLCFIClientStateManager()
        {
            Facilities = new TLCFacilities();
            InternalIntersections = new List<Intersection>();
            InternalVariables = new List<Variable>();
            InternalSignalGroups = new List<SignalGroup>();
            InternalDetectors = new List<Detector>();
            InternalInputs = new List<Input>();
            InternalOutputs = new List<Output>();
            SpvhGenerator = new SpecialVehicleEventGenerator();
        }

        #endregion // Constructor
    }
}