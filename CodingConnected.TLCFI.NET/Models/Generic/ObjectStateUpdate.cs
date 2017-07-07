using CodingConnected.TLCFI.NET.Models.Converters;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    [JsonConverter(typeof(TlcObjectJsonConverter))]
    public class ObjectStateUpdate
    {
        #region Properties

        [JsonProperty("objects")]
        public ObjectReference Objects { get; set; }

        [JsonProperty("states")]
        public object [] States { get; set; } // ObjectType in IDD: ObjectStateUpdateContent

        #endregion // Properties
    }
}
