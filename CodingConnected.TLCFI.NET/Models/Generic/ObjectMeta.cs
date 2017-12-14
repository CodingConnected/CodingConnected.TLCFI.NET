using CodingConnected.TLCFI.NET.Core.Models.Converters;
using CodingConnected.TLCFI.NET.Core.Models.Converters;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.Generic
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
