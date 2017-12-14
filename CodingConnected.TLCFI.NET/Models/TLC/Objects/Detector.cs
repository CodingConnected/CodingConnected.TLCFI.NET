using System;
using System.Dynamic;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using CodingConnected.TLCFI.NET.Core.Tools;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public class Detector : TLCIntersectionObjectBase
    {
        #region Fields

        private string _id;
        private DetectorState? _state;
        private DetectorFaultState? _faultState;
        private SwicoState? _swico;

        private bool _stateChanged;
        private bool _faultStateChanged;
        private bool _swicoChanged;

        #endregion // Fields

        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.Detector;

        [JsonIgnore]
        public override bool StateChanged => _stateChanged ||
                                             _faultStateChanged ||
                                             _swicoChanged;

        // META

        [ObjectId]
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
        [JsonProperty("generatesEvents")]
        public bool GeneratesEvents { get; set; }

        // STATE

        /// <summary>
        /// Defines the tick of the TLC Facilities when one or more
        /// state attributes were last changed
        /// </summary>
        [JsonProperty("stateticks")]
        public uint StateTicks { get; set; } // Ticks

        [JsonProperty("state")]
        public DetectorState? State
        {
            get => _state;
            set
            {
                _state = value;
                _stateChanged = true;
                ChangedState?.Invoke(this, EventArgs.Empty);
                StateTicks = TicksGenerator.Default.GetCurrentTicks();
            }
        }

        [JsonProperty("faultstate")]
        public DetectorFaultState? FaultState
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
                id = Id,
                generatesEvents = GeneratesEvents
            };
        }

        public override void CopyState(object o)
        {
            var d = o as Detector;
            if (d == null)
            {
                throw new InvalidCastException();
            }
            StateTicks = d.StateTicks;
            if (d.State.HasValue) State = d.State;
            if (d.FaultState.HasValue) FaultState = d.FaultState;
            if (d.Swico.HasValue) Swico = d.Swico;
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
