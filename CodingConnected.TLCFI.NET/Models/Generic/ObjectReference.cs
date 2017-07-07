using System.Text;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public class ObjectReference
    {
        #region Fields

        private string[] _ids;
        
        #endregion // Fields
        
        #region Properties

        [JsonProperty("type")]
        public TLC.TLCObjectType Type { get; set; } // This is abstract in the spec: for TLC-FI, type is TLCObjectType

        [JsonProperty("ids")]
        public string[] Ids
        {
            get => _ids;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _ids = value;
            }
        }

        #endregion // Properties

        #region Overrides

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ObjectReference: type=" + Type + " ids=");
            foreach (var s in Ids)
            {
                sb.Append(s + ";");
            }
            return sb.ToString();
        }

        #endregion // Overrides
    }
}
