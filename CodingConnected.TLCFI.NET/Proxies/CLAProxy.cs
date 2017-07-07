using CodingConnected.JsonRPC;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Data;
using CodingConnected.TLCFI.NET.Generic;
using CodingConnected.TLCFI.NET.Models.Generic;
using CodingConnected.TLCFI.NET.Models.TLC;

namespace CodingConnected.TLCFI.NET.Proxies
{
    public sealed class CLAProxy : ITLCFIClientAsync, IDisposable
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly JsonRpcClient _rpcClient;

        private readonly int _maxRpcDuration = TLCFIDataProvider.Default.Settings.MaxRpcDuration;
        private readonly int _maxRpcDurationSession = TLCFIDataProvider.Default.Settings.MaxRpcDurationSession;
        private readonly int _aliveReceiveTimeOut = TLCFIDataProvider.Default.Settings.AliveReceiveTimeOut;

        #endregion // Fields

        #region Private Properties

        private TwoWayTcpClient Client { get; }

        #endregion // Private Properties

        #region ITLCFIClient

#warning Compare TLCProxy: need more error handling code; also: use all three times ints from above
        public async Task UpdateStateAsync(ObjectStateUpdateGroup objectstateupdategroup, CancellationToken token)
        {
            try
            {
                await _rpcClient.InvokeAsync("UpdateState", objectstateupdategroup, _maxRpcDuration, CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.Error("Calling method ReadMeta failed; see trace for details");
                _logger.Trace(e, "Calling method ReadMeta failed with exception: ");
            }
        }

        public async Task NotifyEventAsync(ObjectEvent objectevent, CancellationToken token)
        {
            try
            {
                await _rpcClient.InvokeAsync("NotifyEvent", objectevent, _maxRpcDuration, CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.Error("Calling method NotifyEvent failed; see trace for details");
                _logger.Trace(e, "Calling method NotifyEvent failed with exception: ");
            }
        }
        
        public async Task<AliveObject> AliveAsync(AliveObject alive, CancellationToken token)
        {
            try
            {
                return await _rpcClient.InvokeAsync<AliveObject>("Alive", alive, _maxRpcDuration, CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.Error("Calling method Alive failed; see trace for details");
                _logger.Trace(e, "Calling method Alive failed with exception: ");
                return null;
            }
        }

        public async Task<ObjectMeta> ReadMetaAsync(ObjectReference objectReference, CancellationToken token)
        {
            try
            {
                return await _rpcClient.InvokeAsync<ObjectMeta>("ReadMeta", objectReference, _maxRpcDuration, CancellationToken.None);
            }
            catch (Exception e)
            {
                _logger.Error("Calling method ReadMeta failed; see trace for details");
                _logger.Trace(e, "Calling method ReadMeta failed with exception: ");
                return null;
            }
        }

        #endregion // ITLCFIFacilities

        #region Public Methods

        #endregion // Public Methods

        #region Private Methods

        #endregion // Private Methods

        #region IDisposable

        public void Dispose()
        {
            Client.DataReceived -= _rpcClient.HandleDataReceived;
        }

        #endregion // IDisposable

        #region Constructor

        public CLAProxy(TwoWayTcpClient client, int maxRpcDuration, int maxRpcDurationSession)
        {
            Client = client;
            _rpcClient.TcpClient = Client;
            Client.DataReceived += _rpcClient.HandleDataReceived;

            _maxRpcDuration = maxRpcDuration;
            _maxRpcDurationSession = maxRpcDurationSession;
        }

        static CLAProxy()
        {
            _rpcClient = new JsonRpcClient();
        }

        #endregion // Constructor
    }
}
