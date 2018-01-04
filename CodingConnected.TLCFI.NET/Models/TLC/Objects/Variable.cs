using System;
using System.Dynamic;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public class Variable : TLCObjectBase
    {
        #region Fields

        private bool _valueChanged;
        private bool _reqValueChanged;
        private bool _lifetimeChanged;
        private bool _reqLifetimeChanged;

        #endregion // Fields

        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.Variable;

        [JsonIgnore]
        public override bool StateChanged => _valueChanged ||
                                             _reqValueChanged ||
                                             _lifetimeChanged ||
                                             _reqLifetimeChanged;

        // META

        private string _id;
        private int? _value;
        private int? _reqValue;
        private int? _lifetime;
        private int? _reqLifetime;

        [JsonProperty("id")]
        public override string Id
        {
            get => _id;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _id = value;
            }
        }

        // STATE

        [JsonProperty("value")]
        public int? Value
        {
            get => _value;
            set
            {
                Tools.ValueChecker.CheckValidVariableState(value);
                ChangedState?.Invoke(this, EventArgs.Empty);
                _value = value;
            }
        }

        [JsonProperty("reqValue")]
        public int? ReqValue
        {
            get => _reqValue;
            set
            {
                Tools.ValueChecker.CheckValidVariableState(value);
                _reqValue = value; 
            }
        }

        [JsonProperty("lifetime")]
        public int? Lifetime
        {
            get => _lifetime;
            set
            {
                Tools.ValueChecker.CheckValidVariableLifetime(value);
                ChangedState?.Invoke(this, EventArgs.Empty);
	            _lifetime = value;
			}
        }

        [JsonProperty("reqLifetime")]
        public int? ReqLifetime
        {
            get => _reqLifetime;
            set
            {
                Tools.ValueChecker.CheckValidVariableLifetime(value);
                _reqLifetime = value; 
            }
        }

        #endregion // Properties

        #region Events

        public event EventHandler ChangedState;

        #endregion // Events

        #region TLCObjectBase Methods

        public override void ResetChanged()
        {
            _valueChanged = false;
            _reqValueChanged = false;
            _lifetimeChanged = false;
            _reqLifetimeChanged = false;
        }

        public override object GetMeta()
        {
            return new
            {
                Id
            };
        }

        public override void CopyState(object o)
        {
            var sg = o as Variable;
            if (sg == null)
            {
                throw new InvalidCastException();
            }
            if (sg.Value.HasValue) Value = sg.Value;
            if (sg.ReqValue.HasValue) ReqValue = sg.ReqValue;
            if (sg.Lifetime.HasValue) Lifetime = sg.Lifetime;
            if (sg.ReqLifetime.HasValue) ReqLifetime = sg.ReqLifetime;
        }

        public override object GetState(bool tlc = false)
        {
            dynamic state = new ExpandoObject();
            if (tlc)
            {
                if (_valueChanged) state.value = Value;
                if (_reqValueChanged) state.reqValue = ReqValue;
                if (_lifetimeChanged) state.lifetime = Lifetime;
                if (_reqLifetimeChanged) state.reqLifetime = ReqLifetime;
            }
            else
            {
                if (_reqValueChanged) state.reqValue = ReqValue;
                if (_reqLifetimeChanged) state.reqLifetime = ReqLifetime;
            }
            return state;
        }

        public override object GetFullState(bool tlc = false)
        {
            if (tlc)
            {
                return new
                {
                    value = Value,
                    reqValue = ReqValue,
                    lifetime = Lifetime,
                    reqLifetime = ReqLifetime
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
