using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public class SignalConflict
    {
        #region Fields

        private string _signalGroup;

        #endregion // Fields

        #region Properties

        [JsonProperty("signalgroup")]
        public string SignalGroup
        {
            get => _signalGroup;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _signalGroup = value;
            }
        }

        [JsonProperty("intergreentime")]
        public int InterGreenTime { get; set; }
        
        #endregion // Properties
    }
}
