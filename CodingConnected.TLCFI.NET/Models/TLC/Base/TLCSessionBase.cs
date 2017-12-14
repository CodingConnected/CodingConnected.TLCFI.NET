using CodingConnected.TLCFI.NET.Core.Models.Generic;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC.Base
{
    public abstract class TLCSessionBase : TLCObjectBase
    {
        #region Properties

        public abstract ApplicationType? SessionType { get; }

        #endregion // Properties
    }
}
