using System;

namespace CodingConnected.TLCFI.NET.Data
{
    [Serializable]
    public class TLCFISettings
    {
        #region Properties

        public int AliveSendTimeOut { get; set; }
        public int AliveReceiveTimeOut { get; set; }

        public int MaxRpcDuration { get; set; }
        public int MaxRpcDurationSession { get; set; }
        
        public int MaxReleaseControlDuration { get; set; }

        #endregion // Properties
    }
}
