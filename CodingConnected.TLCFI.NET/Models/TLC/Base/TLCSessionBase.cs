using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Models.TLC.Base
{
    public abstract class TLCSessionBase : TLCObjectBase
    {
        #region Properties

        public abstract ApplicationType SessionType { get; }

        #endregion // Properties
    }
}
