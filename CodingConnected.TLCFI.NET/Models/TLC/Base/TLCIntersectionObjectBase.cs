using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public abstract class TLCIntersectionObjectBase : TLCObjectBase
    {
        #region Properties

        public override string Id { get; set; }

        // NON-FI

        private string _intersectionid;
        [JsonIgnore]
        public string IntersectionId
        {
            get => _intersectionid;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _intersectionid = value;
            }
        }

        #endregion // Properties
    }
}
