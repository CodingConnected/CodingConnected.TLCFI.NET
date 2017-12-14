using CodingConnected.TLCFI.NET.Core.Models.TLC;
using CodingConnected.TLCFI.NET.Core.Tools;
using CodingConnected.TLCFI.NET.Core.Tools;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    public class SessionEventInformation
    {
        #region Fields

        private string _id;
        
        #endregion // Fields

        #region Properties

        [JsonProperty("type")]
        public TLCObjectType Type { get; set; }

        [JsonProperty("id")]
        public string Id
        {
            get => _id;
            set
            {
                ValueChecker.CheckValidObjectId(value);
                _id = value; 
            }
        }

        [JsonProperty("attribute")]
        public string Attribute { get; set; }

        #endregion // Properties
    }
}