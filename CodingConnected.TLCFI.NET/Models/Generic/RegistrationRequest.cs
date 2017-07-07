using System;
using CodingConnected.TLCFI.NET.Tools;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public class RegistrationRequest
    {
        #region Fields

        private string _password;
        private string _username;

        #endregion // Fields

        #region Properties

        [JsonProperty("username")]
        public string Username
        {
            get => _username;
            set
            {
                ValueChecker.CheckValidApplicationString(value);
                _username = value; 
            }
        }

        [JsonProperty("password")]
        public string Password
        {
            get => _password;
            set
            {
                ValueChecker.CheckValidApplicationString(value);
                _password = value; 
            }
        }

        [JsonProperty("type")]
        public ApplicationType Type { get; set; }

        [JsonProperty("version")]
        public ProtocolVersion Version { get; set; }

        [JsonProperty("uri")]
        public Uri Uri { get; set; }
        
        #endregion // Properties
    }
}
