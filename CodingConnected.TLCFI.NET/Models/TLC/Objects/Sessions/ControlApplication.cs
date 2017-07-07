using System;
using System.Dynamic;
using CodingConnected.TLCFI.NET.Models.Generic;
using CodingConnected.TLCFI.NET.Models.TLC.Base;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class ControlApplication : TLCSessionBase
    {
        #region Fields

        private string _id;
        private string _reqIntersection;
        private HandoverCapability? _startCapability;
        private HandoverCapability? _endCapability;
        private ControlState? _reqControlState;
        private ControlState? _controlState;
        private HandoverCapability? _reqHandover;

        private bool _startCapabilityChanged;
        private bool _endCapabilityChanged;
        private bool _reqIntersectionChanged;
        private bool _reqControlStateChanged;
        private bool _controlStateChanged;
        private bool _reqHandoverChanged;

        #endregion // Fields

        #region Properties

        // NON FI

        [JsonIgnore]
        public override ApplicationType SessionType => ApplicationType.Control;

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.Session;

        [JsonIgnore]
        public override bool StateChanged => _startCapabilityChanged ||
                                             _endCapabilityChanged ||
                                             _reqIntersectionChanged ||
                                             _reqControlStateChanged ||
                                             _controlStateChanged ||
                                             _reqHandoverChanged;

        // META

        [ObjectID]
        [JsonProperty("sessionid")]
        public override string Id // TLC-FI type: ObjectID | name: sessionid
        {
            get => _id;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _id = value;
            }
        }
        [JsonProperty("type")]
        public ApplicationType Type { get; set; }

        // STATE

        [JsonProperty("startCapability")]
        public HandoverCapability? StartCapability
        {
            get => _startCapability;
            set
            {
                _startCapability = value;
                _startCapabilityChanged = true;
            }
        }

        [JsonProperty("endCapability")]
        public HandoverCapability? EndCapability
        {
            get => _endCapability;
            set
            {
                _endCapability = value;
                _endCapabilityChanged = true;
            }
        }

        [JsonProperty("reqIntersection")]
        public string ReqIntersection
        {
            get => _reqIntersection;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _reqIntersection = value;
                _reqIntersectionChanged = true;
            }
        }

        [JsonProperty("reqControlState")]
        public ControlState? ReqControlState
        {
            get => _reqControlState;
            set
            {
                _reqControlState = value;
                _reqControlStateChanged = true;
            }
        }

        [JsonProperty("controlState")]
        public ControlState? ControlState
        {
            get => _controlState;
            set
            {
                _controlState = value;
                _controlStateChanged = true;
                HasControlStateChanged?.Invoke(this, value);
            }
        }

        [JsonProperty("reqHandover")]
        public HandoverCapability? ReqHandover
        {
            get => _reqHandover;
            set
            {
                _reqHandover = value;
                _reqHandoverChanged = true;
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler<ControlState?> HasControlStateChanged;

        #endregion // Events

        #region TLCObjectBase Methods

        public override void ResetChanged()
        {
            _startCapabilityChanged = false;
            _endCapabilityChanged = false;
            _reqIntersectionChanged = false;
            _reqControlStateChanged = false;
            _controlStateChanged = false;
            _reqHandoverChanged = false;
        }

        public override object GetMeta()
        {
            return new
            {
                sessionid = Id,
                type = Type
            };
        }


        public override void CopyState(object o)
        {
            throw new NotImplementedException();
        }

        public override object GetState(bool tlc = false)
        {
            dynamic state = new ExpandoObject();

            if (tlc)
            {
                {
                    if (_controlStateChanged)
                    {
                        state.controlState = ControlState;
                    }
                    if (_reqHandoverChanged)
                    {
                        state.reqHandover = ReqHandover;
                    }
                }
            }
            else
            {
                if (_startCapabilityChanged)
                {
                    state.startCapability = StartCapability;
                }
                if (_endCapabilityChanged)
                {
                    state.endCapability = EndCapability;
                }
                if (_reqIntersectionChanged)
                {
                    state.reqIntersection = ReqIntersection;
                }
                if (_reqControlStateChanged)
                {
                    state.reqControlState = ReqControlState;
                }
            }
            return state;
        }

        public override object GetFullState(bool tlc = false)
        {
            if (tlc)
            {
                return new
                {
                    controlState = ControlState,
                    reqHandover = ReqHandover
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
