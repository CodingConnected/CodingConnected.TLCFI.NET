using CodingConnected.TLCFI.NET.Core.Tools;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public class FacilitiesInformation
    {
        private string _companyName;
        private string _facilitiesVersion;

        #region Properties

        [JsonProperty("fiVersion")]
        public Generic.ProtocolVersion FiVersion { get; set; }

        [JsonProperty("companyname")]
        public string CompanyName
        {
            get => _companyName;
            set
            {
                ValueChecker.CheckValidCompanyName(value);
                _companyName = value; 
            }
        }

        [JsonProperty("facilitiesVersion")]
        public string FacilitiesVersion
        {
            get => _facilitiesVersion;
            set
            {
                ValueChecker.CheckValidFacilitiesVersion(value);
                _facilitiesVersion = value; 
            }
        }

        #endregion // Properties
    }
}
