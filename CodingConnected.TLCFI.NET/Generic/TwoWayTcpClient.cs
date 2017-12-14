using NLog;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.JsonRPC;

namespace CodingConnected.TLCFI.NET.Core.Generic
{
    public sealed partial class TwoWayTcpClient : IDisposable, ITcpClient
    {
        #region Fields

        private TcpClient _client;
        private NetworkStream _stream;
        private Receiver _receiver;
        private Sender _sender;
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        #endregion // Fields

        #region Properties

        public bool Connected => _client?.Client != null && _client.Connected;
        public IPEndPoint LocalEndPoint => _client?.Client?.LocalEndPoint as IPEndPoint;

        #endregion // Properties

        #region Public Methods

        public async Task SendDataAsync(string data, CancellationToken token)
        {
            if (Connected)
            {
                await _sender.SendDataAsync(data, token);
            }
            else
            {
                _logger.Trace("SendDataAsync called, but not connected to peer.");
            }
        }

        public async Task ConnectAsync(IPEndPoint ep, int timeout, CancellationToken token)
        {
            await Task.Factory.StartNew(() => { Connect(ep, timeout); }, token);
        }

        private void Connect(IPEndPoint ep, int timeout)
        {
            _client = new TcpClient
            {
                ReceiveBufferSize = 32768,
                SendBufferSize = 32768
            };
            var result = _client.BeginConnect(ep.Address, ep.Port, null, null);
            var success = result.AsyncWaitHandle.WaitOne(timeout);
            if (!success)
            {
                _client.Close();
                throw new Exception("Failed to connect.");
            }
            _client.EndConnect(result);
            if (!_client.Client.Connected)
            {
                _client.Close();
                throw new Exception("Failed to connect.");
            }
            SetupClient();
        }

        #endregion // Public Methods

        #region Private Methods

        private void SetupClient()
        {
            _stream = _client.GetStream();

            _receiver = new Receiver(_stream);
            _sender = new Sender(_stream);

            _sender.DataSent += DataSent;
            _receiver.DataReceived += (o, e) => { DataReceived?.Invoke(this, e); };
            _sender.Disconnected += (o, e) => { Disconnected?.Invoke(this, e); };
            _receiver.Disconnected += (o, e) => { Disconnected?.Invoke(this, e); };
        }
        
        #endregion // Private Methods

        #region Events

        public event EventHandler<string> DataReceived;
        public event EventHandler<string> DataSent;
        public event EventHandler Disconnected;
        
        #endregion // Events

        #region Constructors

        public TwoWayTcpClient()
        {

        }

        public TwoWayTcpClient(TcpClient client)
        {
            _client = client;
            SetupClient();
        }

        #endregion // Constructors

        #region IDisposable

        public void Dispose()
        {
            _client?.Client?.Close();
            _client?.Close();
            if (_receiver != null) _receiver.Disposed = true;
            if (_sender != null) _sender.Disposed = true;
        }

        #endregion // IDisposable
    }
}
