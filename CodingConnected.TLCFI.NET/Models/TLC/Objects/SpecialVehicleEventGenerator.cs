using System;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using CodingConnected.TLCFI.NET.Tools;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class SpecialVehicleEventGenerator : TLCObjectBase
    {
        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.SpecialVehicleEventGenerator;

        [JsonIgnore]
        public override bool StateChanged => _faultStateChanged;
        // META

        private string _id;
        private bool _faultStateChanged;
        private SpecialVehicleEventGeneratorFaultState? _faultState;

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

        [JsonProperty("faultstate")]
        public SpecialVehicleEventGeneratorFaultState? FaultState
        {
            get => _faultState;
            set
            {
                _faultState = value; 
                _faultStateChanged = true;
            }
        }

        #endregion // Properties

        #region TLCObjectBase Methods

        public override void ResetChanged()
        {
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
            var sp = o as SpecialVehicleEventGenerator;
            if (sp == null)
            {
                throw new InvalidCastException();
            }
            if (sp.FaultState.HasValue) FaultState = sp.FaultState;
        }

        public override object GetState(bool tlc = false)
        {
            dynamic state = new ExpandoObject();
            if (tlc)
            {
                if (_faultStateChanged) state.faultstate = FaultState;
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
                    faultState = FaultState
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
