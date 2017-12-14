using CodingConnected.TLCFI.NET.Core.Models.Converters;
using CodingConnected.TLCFI.NET.Core.Models.Converters;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    [JsonConverter(typeof(TlcObjectJsonConverter))]
    public class ObjectData
    {
        #region Properties

        [JsonProperty("objects")]
        public ObjectReference Objects { get; set; }

        [JsonProperty("data")]
        public object [] Data { get; set; } // ObjectType in IDD: ObjectDataContent

        [JsonProperty("ticks")]
        public uint Ticks { get; set; }
        
        #endregion // Properties
    }
}
