using System;
using System.Dynamic;
using CodingConnected.TLCFI.NET.Tools;
using Newtonsoft.Json;
using NLog;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class Input : TLCIntersectionObjectBase
    {
        #region Properties

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _stateChanged;
        private bool _faultStateChanged;
        private bool _swicoChanged;

        #endregion // Properties

        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.Input;

        [JsonIgnore]
        public override bool StateChanged => _stateChanged ||
                                             _faultStateChanged ||
                                             _swicoChanged;

        // META

        private string _id;
        private int? _state;
        private InputFaultState? _faultState;
        private SwicoState? _swico;

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

        // STATE

        /// <summary>
        /// Defines the tick of the TLC Facilities when one or more
        /// state attributes were last changed
        /// </summary>
        [JsonProperty("stateticks")]
        public uint StateTicks { get; set; }

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

        [JsonProperty("faultstate")]
        public InputFaultState? FaultState
        {
            get => _faultState;
            set
            {
                _faultState = value;
                _faultStateChanged = true;
                StateTicks = TicksGenerator.Default.GetCurrentTicks();
            }
        }

        [JsonProperty("swico")]
        public SwicoState? Swico
        {
            get => _swico;
            set
            {
                _swico = value;
                _swicoChanged = true;
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
            _faultStateChanged = false;
            _swicoChanged = false;
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
            var i = o as Input;
            if (i == null)
            {
                throw new InvalidCastException();
            }
            StateTicks = i.StateTicks;
            if (i.State.HasValue) State = i.State;
            if (i.FaultState.HasValue) FaultState = i.FaultState;
            if (i.Swico.HasValue) Swico = i.Swico;
        }

        public override object GetState(bool tlc = false)
        {
            dynamic state = new ExpandoObject();
            if (tlc)
            {
                state.stateticks = StateTicks;
                if (_stateChanged) state.state = State;
                if (_faultStateChanged) state.faultstate = FaultState;
                if (_swicoChanged) state.swico = Swico;
            }
            else
            {
                throw new UnauthorizedAccessException();
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
                    swico = Swico
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
