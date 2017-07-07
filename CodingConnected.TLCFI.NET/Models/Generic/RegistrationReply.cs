using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public class RegistrationReply
    {
        #region Fields

        private string _sessionid;

        #endregion // Fields

        #region Properties

        [JsonProperty("sessionid")]
        public string Sessionid
        {
            get => _sessionid;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _sessionid = value;
            }
        }

        [JsonProperty("facilities")]
        public ObjectReference Facilities { get; set; }

        [JsonProperty("version")]
        public ProtocolVersion Version { get; set; }
        
        #endregion // Properties
    }
}
