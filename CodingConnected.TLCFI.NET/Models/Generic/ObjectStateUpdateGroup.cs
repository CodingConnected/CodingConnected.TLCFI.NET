using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public class ObjectStateUpdateGroup
    {
        #region Properties

        [JsonProperty("update")]
        public ObjectStateUpdate [] Update { get; set; }

        [JsonProperty("ticks")]
        public uint Ticks { get; set; }
        
        #endregion // Properties
    }
}
