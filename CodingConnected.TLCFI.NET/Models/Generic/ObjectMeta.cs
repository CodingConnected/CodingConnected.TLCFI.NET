using CodingConnected.TLCFI.NET.Models.Converters;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    [JsonConverter(typeof(TlcObjectJsonConverter))]
    public class ObjectMeta
    {
        #region Properties

        [JsonProperty("objects")]
        public ObjectReference Objects { get; set; }

        [JsonProperty("meta")]
        public object [] Meta { get; set; } // ObjectType in IDD: ObjectMetaContent

        [JsonProperty("ticks")]
        public uint Ticks { get; set; }

        #endregion // Properties
    }
}
