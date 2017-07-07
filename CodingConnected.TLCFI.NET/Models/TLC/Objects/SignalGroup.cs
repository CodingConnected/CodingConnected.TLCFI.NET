using System;
using System.Dynamic;
using CodingConnected.TLCFI.NET.Tools;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class SignalGroup : TLCIntersectionObjectBase
    {
        #region Fields


        private bool _stateChanged;
        private bool _reqStateChanged;
        private bool _predictionsChanged;
        private bool _reqPredictionsChanged;

        #endregion // Fields

        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.SignalGroup;

        [JsonIgnore]
        public override bool StateChanged => _stateChanged ||
                                             _reqStateChanged ||
                                             _predictionsChanged ||
                                             _reqPredictionsChanged;

        // META

        private string _id;
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

        private string _intersection;
        private SignalGroupState? _reqState;
        private SignalGroupState? _state;
        private SignalGroupPrediction[] _reqPredictions;
        private SignalGroupPrediction[] _predictions;

        [JsonProperty("intersection")]
        public string Intersection
        {
            get => _intersection;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _intersection = value;
            }
        }

        [JsonProperty("intergreen")]
        public SignalConflict [] Intergreen { get; set; }

        [JsonProperty("timing")]
        public SignalTiming [] Timing { get; set; }

        // STATE

        [JsonProperty("stateticks")]
        public uint StateTicks { get; set; }

        [JsonProperty("reqState")]
        public SignalGroupState? ReqState
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
        public SignalGroupState? State
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

        [JsonProperty("reqPredictions")]
        public SignalGroupPrediction[] ReqPredictions
        {
            get => _reqPredictions;
            set
            {
                if (value?.Length > 16)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _reqPredictions = value;
                _reqPredictionsChanged = true;
                StateTicks = TicksGenerator.Default.GetCurrentTicks();
            }
        }

        [JsonProperty("predictions")]
        public SignalGroupPrediction[] Predictions
        {
            get => _predictions;
            set
            {
                if (value?.Length > 16)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _predictions = value;
                _predictionsChanged = true;
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
            _predictionsChanged = false;
            _reqPredictionsChanged = false;
        }

        public override object GetMeta()
        {
            return new
            {
                id = Id,
                intersection = Intersection,
                intergreen = Intergreen,
                timing = Timing,
            };
        }

        public override void CopyState(object o)
        {
            var sg = o as SignalGroup;
            if (sg == null)
            {
                throw new InvalidCastException();
            }
            StateTicks = sg.StateTicks;
            if (sg.State.HasValue) State = sg.State;
            if (sg.ReqState.HasValue) ReqState = sg.ReqState;
            if (sg.Predictions?.Length > 0) Predictions = sg.Predictions;
            if (sg.ReqPredictions?.Length > 0) ReqPredictions = sg.ReqPredictions;
        }

        public override object GetState(bool tlc = false)
        {
            dynamic state = new ExpandoObject();
            if (tlc)
            {
                state.stateticks = StateTicks;
                if (_stateChanged) state.state = State;
                if (_reqStateChanged) state.reqState = ReqState;
                if (_predictionsChanged) state.predictions = Predictions;
                if (_reqPredictionsChanged) state.reqPredictions = ReqPredictions;
            }
            else
            {
                if (_reqStateChanged) state.reqState = ReqState;
                if (_reqPredictionsChanged) state.reqPredictions = ReqPredictions;
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
                    reqState = ReqState,
                    prediction = Predictions,
                    reqPredictions = ReqPredictions
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
