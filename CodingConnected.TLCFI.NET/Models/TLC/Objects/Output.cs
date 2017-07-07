using System;
using System.Dynamic;
using CodingConnected.TLCFI.NET.Tools;
using Newtonsoft.Json;
using NLog;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class Output : TLCIntersectionObjectBase
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _stateChanged;
        private bool _reqStateChanged;
        private bool _faultStateChanged;

        #endregion // Fields

        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.Output;

        [JsonIgnore]
        public override bool StateChanged => _stateChanged ||
                                             _reqStateChanged ||
                                             _faultStateChanged;

        [JsonIgnore]
        public bool Exclusive { get; set; }

        // META

        private string _id;
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
        private OutputFaultState? _faultState;
        private int? _state;
        private int? _reqState;

        // STATE

        [JsonProperty("stateticks")]
        public uint StateTicks { get; set; }

        [JsonProperty("reqState")]
        public int? ReqState
        {
            get => _reqState;
            set
            {
                if (value.HasValue && value.Value > -32768 && value.Value < 32768)
                {
                    _reqState = value;
                    _reqStateChanged = true;
                    StateTicks = TicksGenerator.Default.GetCurrentTicks();
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [JsonProperty("state")]
        public int? State
        {
            get => _state;
            set
            {
                if (value.HasValue && value.Value > -32768 && value.Value < 32768)
                {
                    _state = value;
                    _stateChanged = true;
                    ChangedState?.Invoke(this, EventArgs.Empty);
                    StateTicks = TicksGenerator.Default.GetCurrentTicks();
                }
                else
                {
                    _logger.Warn("Output.State set to invalid value: {0}", value);
                }
            }
        }
        
        [JsonProperty("faulsState")]
        public OutputFaultState? FaultState
        {
            get => _faultState;
            set
            {
                _faultState = value;
                _faultStateChanged = true;
                StateTicks = TicksGenerator.Default.GetCurrentTicks();
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler ChangedState;

        #endregion // Events

        #region TLCObjectBase Methods

        public override void ResetChanged()
        {
            _stateChanged = false;
            _reqStateChanged = false;
            _faultStateChanged = false;
        }

        public override object GetMeta()
        {
            return new
            {
                id = Id
            };
        }

        public override void CopyState(object o)
        {
            var op = o as Output;
            if (op == null)
            {
                throw new InvalidCastException();
            }
            StateTicks = op.StateTicks;
            if (op.State.HasValue) State = op.State;
            if (op.FaultState.HasValue) FaultState = op.FaultState;
            if (op.ReqState.HasValue) ReqState = op.ReqState;
        }

        public override object GetState(bool tlc = false)
        {
            dynamic state = new ExpandoObject();
            if (tlc)
            {
                state.stateticks = StateTicks;
                if (_stateChanged) state.state = State;
                if (_faultStateChanged) state.faultstate = FaultState;
                if (_reqStateChanged) state.reqState = ReqState;
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
                    faultstate = FaultState,
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
