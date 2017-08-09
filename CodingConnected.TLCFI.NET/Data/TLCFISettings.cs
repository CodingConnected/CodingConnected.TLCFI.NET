using System;
using JetBrains.Annotations;

namespace CodingConnected.TLCFI.NET.Data
{
    [Serializable]
    public class TLCFISettings
    {
        #region Properties

        [UsedImplicitly]
        public int AliveSendTimeOut { get; set; }
        [UsedImplicitly]
        public int AliveReceiveTimeOut { get; set; }

        [UsedImplicitly]
        public int MaxRpcDuration { get; set; }
        [UsedImplicitly]
        public int MaxRpcDurationSession { get; set; }
        
        [UsedImplicitly]
        public int MaxReleaseControlDuration { get; set; }

        [UsedImplicitly]
        public bool LogAliveTrace { get; set; }

        #endregion // Properties
    }
}
