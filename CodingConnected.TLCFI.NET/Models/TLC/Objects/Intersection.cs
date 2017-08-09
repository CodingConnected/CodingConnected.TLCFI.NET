using System;
using System.Dynamic;
using CodingConnected.TLCFI.NET.Tools;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class Intersection : TLCObjectBase
    {
        #region Fields

        private string _id;
        private string[] _outputs;
        private string[] _inputs;
        private string[] _signalgroups;
        private string[] _detectors;
        private string _spvehgenerator;

        private bool _stateChanged;
        private bool _reqStateChanged;
        private IntersectionControlState? _reqState;
        private IntersectionControlState? _state;

        #endregion // Fields

        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.Intersection;

        [JsonIgnore]
        public override bool StateChanged => _stateChanged ||
                                             _reqStateChanged;

        // META

        [ObjectID]
        [JsonProperty("id")]
        public override string Id
        {
            get => _id;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _id = value;
            }
        }
        [JsonProperty("outputs")]
        public string [] Outputs
        {
            get => _outputs;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _outputs = value;
            }
        }
        [JsonProperty("inputs")]
        public string[] Inputs
        {
            get => _inputs;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _inputs = value;
            }
        }
        [JsonProperty("signalgroups")]
        public string [] Signalgroups
        {
            get => _signalgroups;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _signalgroups = value;
            }
        }
        [JsonProperty("detectors")]
        public string [] Detectors
        {
            get => _detectors;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _detectors = value;
            }
        }
        [JsonProperty("spvehgenerator")]
        public string Spvehgenerator
        {
            get => _spvehgenerator;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _spvehgenerator = value;
            }
        }

        // STATE

        /// <summary>
        /// Defines the tick of the TLC Facilities when one or more
        /// state attributes were last changed
        /// </summary>
        [JsonProperty("stateticks")]
        public uint StateTicks { get; set; }

        [JsonProperty("reqState")]
        public IntersectionControlState? ReqState
        {
            get => _reqState;
            set
            {
                _reqState = value;
                _reqStateChanged = true;
                StateTicks = TicksGenerator.Default.GetCurrentTicks();
            }
        }

        [JsonProperty("state")]
        public IntersectionControlState? State
        {
            get => _state;
            set
            {
                _state = value;
                _stateChanged = true;
                StateTicks = TicksGenerator.Default.GetCurrentTicks();
                ChangedState?.Invoke(this, value);
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler<IntersectionControlState?> ChangedState;

        #endregion // Events

        #region TLCObjectBase Methods

        public override void ResetChanged()
        {
            _stateChanged = false;
            _reqStateChanged = false;
        }

        public override object GetMeta()
        {
            return new
            {
                id = Id,
                outputs = Outputs,
                inputs = Inputs,
                signalgroups = Signalgroups,
                detectors = Detectors,
                spvehgenerator = Spvehgenerator
            };
        }

        public override void CopyState(object o)
        {
            var i = o as Intersection;
            if (i == null)
            {
                throw new InvalidCastException();
            }
            StateTicks = i.StateTicks;
            if (i.State.HasValue) State = i.State;
            if (i.ReqState.HasValue) ReqState = i.ReqState;
        }

        public override object GetState(bool tlc = false)
        {
            dynamic state = new ExpandoObject();
            if (tlc)
            {
                state.stateticks = StateTicks;
                if (_reqStateChanged) state.reqState = ReqState;
                if (_stateChanged) state.state = State;
            }
            else
            {
                if (_reqStateChanged) state.reqState = ReqState;
            }
            return state;
        }

        public override object GetFullState(bool tlc = false)
        {
            if (tlc)
            {
                return new
                {
                    stateticks = StateTicks,
                    state = State,
                    reqState = ReqState
                };
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }

        #endregion // TLCObjectBase Methods
    }
}
