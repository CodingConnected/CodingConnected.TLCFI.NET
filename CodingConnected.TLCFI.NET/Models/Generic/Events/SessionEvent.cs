using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    public class SessionEvent
    {
        #region Properties

        [JsonProperty("code")]
        public SessionEventCode Code { get; set; }

        [JsonProperty("info")]
        public SessionEventInformation Info { get; set; }
        
        #endregion // Properties
    }
}
