using CodingConnected.JsonRPC;
using NLog;
using System;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Core.Data;
using CodingConnected.TLCFI.NET.Core.Generic;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.TLC;
using CodingConnected.TLCFI.NET.Core.Data;
using CodingConnected.TLCFI.NET.Core.Generic;

namespace CodingConnected.TLCFI.NET.Core.Proxies
{
    public sealed class TLCProxy : ITLCFIFacilitiesAsync, IDisposable
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

        #region ITLCFIFacilities

        public async Task<ObjectData> SubscribeAsync(ObjectReference objectReference, CancellationToken token)
        {
            try
            {
                _logger.Debug("Calling SubscribeAsync with: " + objectReference);
                return await _rpcClient.InvokeAsync<ObjectData>("Subscribe", objectReference, _maxRpcDurationSession, token);
            }
            catch (JsonRpcException e)
            {
                _logger.Error("Calling method SubscribeAsync failed: {0}. See trace for exception details.", e.RpcMessage);
                _logger.Trace(e, "Calling method SubscribeAsync failed:");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Calling method SubscribeAsync failed; see trace for details");
                _logger.Trace(e, "Calling method SubscribeAsync failed: ");
                throw;
            }
        }

        public async Task UpdateStateAsync(ObjectStateUpdateGroup objectstateupdategroup, CancellationToken token)
        {
            try
            {
                await _rpcClient.InvokeAsync("UpdateState", objectstateupdategroup, _maxRpcDuration, token);
            }
            catch (JsonRpcException e)
            {
                _logger.Error("Calling method UpdateStateAsync failed: {0}. See trace for exception details.", e.RpcMessage);
                _logger.Trace(e, "Calling method UpdateStateAsync failed:");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Calling method UpdateStateAsync failed; see trace for details");
                _logger.Trace(e, "Calling method UpdateStateAsync failed: ");
                throw;
            }
        }

        public async Task NotifyEventAsync(ObjectEvent objectevent, CancellationToken token)
        {
            try
            {
                await _rpcClient.InvokeAsync("NotifyEvent", objectevent, _maxRpcDuration, token);
            }
            catch (JsonRpcException e)
            {
                _logger.Error("Calling method NotifyEventAsync failed: {0}. See trace for exception details.", e.RpcMessage);
                _logger.Trace(e, "Calling method NotifyEventAsync failed:");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Calling method NotifyEventAsync failed; see trace for details");
                _logger.Trace(e, "Calling method NotifyEventAsync failed: ");
                throw;
            }
        }

        public async Task<RegistrationReply> RegisterAsync(RegistrationRequest request, CancellationToken token)
        {
            try
            {
                return await _rpcClient.InvokeAsync<RegistrationReply>("Register", request, _maxRpcDurationSession, token);
            }
            catch (JsonRpcException e)
            {
                _logger.Error("Calling method RegisterAsync failed: {0}. See trace for exception details.", e.RpcMessage);
                _logger.Trace(e, "Calling method RegisterAsync failed:");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Calling method RegisterAsync failed; see trace for details");
                _logger.Trace(e, "Calling method RegisterAsync failed: ");
                throw;
            }
        }

        public async Task<DeregistrationReply> DeregisterAsync(DeregistrationRequest request, CancellationToken token)
        {
            try
            {
                _logger.Trace("Calling DeregisterAsync with: " + request);
                return await _rpcClient.InvokeAsync<DeregistrationReply>("Deregister", request, _maxRpcDurationSession, token);
            }
            catch (JsonRpcException e)
            {
                _logger.Error("Calling method DeregisterAsync failed: {0}. See trace for exception details.", e.RpcMessage);
                _logger.Trace(e, "Calling method DeregisterAsync failed:");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Calling method DeregisterAsync failed; see trace for details");
                _logger.Trace(e, "Calling method DeregisterAsync failed: ");
                throw;
            }
        }
        
        public async Task<AliveObject> AliveAsync(AliveObject alive, CancellationToken token)
        {
            try
            {
                return await _rpcClient.InvokeAsync<AliveObject>("Alive", alive, _aliveReceiveTimeOut, token);
            }
            catch (JsonRpcException e)
            {
                _logger.Error("Calling method AliveAsync failed. See trace for exception details.");
                _logger.Trace(e, "Calling method AliveAsync failed: {0}. Original exception: ", e.RpcMessage);
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Calling method AliveAsync failed; see trace for details");
                _logger.Trace(e, "Calling method AliveAsync failed: ");
                throw;
            }
        }

        public async Task<ObjectMeta> ReadMetaAsync(ObjectReference objectReference, CancellationToken token)
        {
            try
            {
                _logger.Trace("Calling ReadMetaAsync with: " + objectReference);
                return await _rpcClient.InvokeAsync<ObjectMeta>("ReadMeta", objectReference, _maxRpcDuration, token);
            }
            catch (JsonRpcException e)
            {
                _logger.Error("Calling method ReadMetaAsync failed: {0}. See trace for exception details.", e.RpcMessage);
                _logger.Trace(e, "Calling method ReadMetaAsync failed:");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error("Calling method ReadMetaAsync failed; see trace for details");
                _logger.Trace(e, "Calling method ReadMetaAsync failed: ");
                throw;
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

        public TLCProxy(TwoWayTcpClient client)
        {
            Client = client;
            _rpcClient.TcpClient = Client;
            Client.DataReceived += _rpcClient.HandleDataReceived;
        }

        static TLCProxy()
        {
            _rpcClient = new JsonRpcClient();
        }

        #endregion // Constructor
    }
}
