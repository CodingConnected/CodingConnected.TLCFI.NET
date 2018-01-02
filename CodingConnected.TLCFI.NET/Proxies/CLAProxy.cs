using CodingConnected.JsonRPC;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Core.Data;
using CodingConnected.TLCFI.NET.Core.Generic;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.TLC;

namespace CodingConnected.TLCFI.NET.Core.Proxies
{
    public sealed class CLAProxy : ITLCFIClientAsync, IDisposable
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly JsonRpcClient _rpcClient;

        private readonly int _maxRpcDuration = TLCFIDataProvider.Default.Settings.MaxRpcDuration;
	    private readonly int _aliveReceiveTimeOut = TLCFIDataProvider.Default.Settings.AliveReceiveTimeOut;

        #endregion // Fields

        #region Private Properties

        private TwoWayTcpClient Client { get; }

        #endregion // Private Properties

        #region ITLCFIClient

        public async Task UpdateStateAsync(ObjectStateUpdateGroup objectstateupdategroup, CancellationToken token)
        {
            try
            {
                await _rpcClient.InvokeAsync("UpdateState", objectstateupdategroup, _maxRpcDuration, CancellationToken.None);
			}
			catch (JsonRpcException e)
			{
				_logger.Error(e, "Calling method UpdateStateAsync failed: {0}. Exception:", e.RpcMessage);
				throw;
			}
			catch (Exception e)
            {
                _logger.Error(e, "Calling method UpdateStateAsync failed with exception: ");
            }
        }

        public async Task NotifyEventAsync(ObjectEvent objectevent, CancellationToken token)
        {
            try
            {
                await _rpcClient.InvokeAsync("NotifyEvent", objectevent, _maxRpcDuration, CancellationToken.None);
			}
			catch (JsonRpcException e)
			{
				_logger.Error(e, "Calling method NotifyEventAsync failed: {0}. Exception: ", e.RpcMessage);
				throw;
			}
			catch (Exception e)
            {
                _logger.Error(e, "Calling method NotifyEventAsync failed with exception: ");
            }
        }
        
        public async Task<AliveObject> AliveAsync(AliveObject alive, CancellationToken token)
        {
            try
            {
                return await _rpcClient.InvokeAsync<AliveObject>("Alive", alive, _aliveReceiveTimeOut, CancellationToken.None);
			}
			catch (JsonRpcException e)
			{
				_logger.Error("Calling method AliveAsync failed: {0}. Exception: ", e.RpcMessage);
				throw;
			}
			catch (Exception e)
            {
				_logger.Error(e, "Calling method AliveAsync failed with exception: ");
				return null;
            }
        }

        public async Task<ObjectMeta> ReadMetaAsync(ObjectReference objectReference, CancellationToken token)
        {
            try
            {
                return await _rpcClient.InvokeAsync<ObjectMeta>("ReadMeta", objectReference, _maxRpcDuration, CancellationToken.None);
			}
			catch (JsonRpcException e)
			{
				_logger.Error(e, "Calling method ReadMetaAsync failed: {0}. Exception: ", e.RpcMessage);
				throw;
			}
			catch (Exception e)
            {
				_logger.Error(e, "Calling method ReadMetaAsync failed with exception: ");
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

        public CLAProxy(TwoWayTcpClient client)
        {
            Client = client;
            _rpcClient.TcpClient = Client;
            Client.DataReceived += _rpcClient.HandleDataReceived;
        }

        static CLAProxy()
        {
            _rpcClient = new JsonRpcClient();
        }

        #endregion // Constructor
    }
}
