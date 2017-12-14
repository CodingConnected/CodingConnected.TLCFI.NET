using System;
using System.Text;
using CodingConnected.TLCFI.NET.Core.Models.Converters;
using CodingConnected.TLCFI.NET.Core.Models.TLC;
using CodingConnected.TLCFI.NET.Core.Models.Converters;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    [JsonConverter(typeof(TlcObjectJsonConverter))]
    public class ObjectEvent
    {
        #region Properties

        [JsonProperty("objects")]
        public ObjectReference Objects { get; set; }

        [JsonProperty("events")]
        public object [] Events { get; set; } // ObjectType in IDD: ObjectEventContent

        [JsonProperty("ticks")]
        public uint Ticks { get; set; }

        #endregion // Properties

        #region Overrides

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Objects);
            sb.Append(" ");
            switch (Objects.Type)
            {
                case TLCObjectType.Detector:
                    foreach (DetectorEvent ev in Events)
                    {
                        sb.Append(ev);
                        sb.Append(" ");
                    }
                    break;
                case TLCObjectType.SpecialVehicleEventGenerator:
                    foreach (SpecialVehicleEvent ev in Events)
                    {
                        sb.Append(ev);
                        sb.Append(" ");
                    }
                    break;
                // Objects below do not generate events
                case TLCObjectType.Session:
                case TLCObjectType.TLCFacilities:
                case TLCObjectType.Intersection:
                case TLCObjectType.SignalGroup:
                case TLCObjectType.Input:
                case TLCObjectType.Output:
                case TLCObjectType.Variable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            sb.Append(" Ticks=" + Ticks);
            return sb.ToString();
        }

        #endregion // Overrides
    }
}
