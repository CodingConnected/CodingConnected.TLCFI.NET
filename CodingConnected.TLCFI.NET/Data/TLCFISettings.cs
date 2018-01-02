using System;
using JetBrains.Annotations;

namespace CodingConnected.TLCFI.NET.Core.Data
{
    [Serializable]
    public class TLCFISettings
    {
        #region Properties

		/// <summary>
		/// Time interval at which consequetive Alive messages are to be sent
		/// </summary>
        [UsedImplicitly]
        public int AliveSendTimeOut { get; set; }

		/// <summary>
		/// Time after which an alive receive error is raised if no Alive message is received
		/// </summary>
        [UsedImplicitly]
        public int AliveReceiveTimeOut { get; set; }

		/// <summary>
		/// Maximum duration a regular RPC call may take
		/// Regular means: not related to session buildup and configuration
		/// </summary>
        [UsedImplicitly]
        public int MaxRpcDuration { get; set; }

		/// <summary>
		/// Maximum duration RPC calls related to session buildup and configuration may take
		/// These are: Register, Deregister, Subscribe
		/// </summary>
		[UsedImplicitly]
        public int MaxRpcDurationSession { get; set; }
        
		/// <summary>
		/// Maximum time releasing session control may take
		/// </summary>
        [UsedImplicitly]
        public int MaxReleaseControlDuration { get; set; }

		/// <summary>
		/// If set to true: Alive messages will be written to the Trace log
		/// </summary>
        [UsedImplicitly]
        public bool LogAliveTrace { get; set; }

        #endregion // Properties
    }
}
