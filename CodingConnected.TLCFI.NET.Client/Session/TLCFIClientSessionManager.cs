using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Client.Data;
using JetBrains.Annotations;
using NLog;

namespace CodingConnected.TLCFI.NET.Client.Session
{
    internal class TLCFIClientSessionManager
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private int _tries;
        private int _timeout = 1000;
        private DateTime _lastSuccesfulRegister;
        private readonly IPEndPoint _endPoint;
        private static TLCFIClientSession _activeSession;
        private static readonly object _locker = new object();
        private CancellationTokenSource _tokenSource;

        #endregion // Fields

        #region Properties
        #endregion // Properties

        #region Events

        [UsedImplicitly]
        public event EventHandler<int> ConnectingStarted;
        [UsedImplicitly]
        public event EventHandler<int> ConnectingFailed;
        [UsedImplicitly]
        public event EventHandler TLCSessionStarted;
        [UsedImplicitly]
        public event EventHandler TLCSessionEnded;

        #endregion // Events

        #region Public Methods

        public async Task<TLCFIClientSession> GetNewSession(TLCFIClientStateManager stateManager, CancellationToken token)
        {
            lock (_locker)
            {
                if (_activeSession != null && !_activeSession.Connected)
                {
                    DisposeActiveSession();
                }
                else if (_activeSession != null)
                {
                    _logger.Warn("There already is a connected session with {0}:{1}. Simultaneous sessions are not allowed.",
                        _activeSession.RemoteEndPoint.Address, _activeSession.RemoteEndPoint.Port);
                    return null;
                }
            }

            // Succesful registration interval
            if (DateTime.Now.Subtract(_lastSuccesfulRegister).TotalSeconds < 42)
            {
                var remaining = (int)(42 - DateTime.Now.Subtract(_lastSuccesfulRegister).TotalSeconds) + 1;
                _logger.Info("Need 42 seconds between succesful register calls. Still need to wait {0} seconds.",
                    remaining);
                await Task.Run(async () =>
                {
                    while (remaining > 0)
                    {
                        await Task.Delay(1000, token);
                        remaining--;
                        if (remaining > 0)
                        {
                            _logger.Trace("Starting new session in {0} seconds.", remaining);
                        }
                    }
                }, token);

            }

            _tokenSource = new CancellationTokenSource();
            var session = new TLCFIClientSession(stateManager, _endPoint, _tokenSource.Token);
            _activeSession = session;
            session.SessionEnded += OnSessionEnded;
            session.Disconnected += OnSessionDisconnected;
            session.ReceiveAliveTimeoutOccured += OnSessionReceiveAliveTimeout;

            var watch = new Stopwatch();
            watch.Reset();

            var sesIp = _endPoint.Address.ToString();
            var sesPort = _endPoint.Port.ToString();

            while (!session.Connected)
            {
                try
                {
                    var remaining = _timeout - watch.ElapsedMilliseconds;
                    if (remaining > 0)
                    {
                        await Task.Delay((int)remaining, token);
                    }
                    _tries++;
                    // Backoff procedure
                    if (_tries >= 25) _timeout = 60000;
                    else if (_tries >= 21) _timeout = 30000;
                    else if (_tries >= 10) _timeout = 5000;
                    else if (_tries >= 5) _timeout = 2000;
                    watch.Reset();
                    watch.Start();
                    _logger.Info("Connecting to {0}:{1}, try {2}", sesIp, sesPort, _tries);
                    ConnectingStarted?.Invoke(this, _tries);
                    if (token.IsCancellationRequested || _tokenSource.IsCancellationRequested)
                        return null;

                    await session.StartSessionAsync(_timeout);
                }
                catch (TaskCanceledException)
                {
                    return null;
                }
                catch
                {
                    _logger.Warn("Connecting to {0}:{1} try {2} failed", sesIp, sesPort, _tries);
                    ConnectingFailed?.Invoke(this, _tries);
                }
            }

            _logger.Info("TCP session with {0}:{1} started", sesIp, sesPort);
            TLCSessionStarted?.Invoke(this, null);

            return session;
        }

        public async Task EndActiveSessionAsync()
        {
            var closeSessionAsync = _activeSession?.CloseSessionAsync();
            if (closeSessionAsync != null) await closeSessionAsync;
            _tokenSource.Cancel();
        }

        public void ResetConnectionRetryTimers()
        {
            _lastSuccesfulRegister = DateTime.Now;
            _tries = 0;
            _timeout = 1000;
        }

        public void DisposeActiveSession()
        {
            lock (_locker)
            {
                if (_activeSession == null)
                {
                    return;
                }

                _activeSession.SessionEnded -= OnSessionEnded;
                _activeSession.Disconnected -= OnSessionDisconnected;
                _activeSession.ReceiveAliveTimeoutOccured -= OnSessionReceiveAliveTimeout;
                _activeSession.DisposeSession();

                _logger.Info("Session with {0}:{1} ended, closed and disposed.",
                    (object) _activeSession.RemoteEndPoint.Address,
                    (object) _activeSession.RemoteEndPoint.Port);

                _tokenSource?.Cancel();

                _activeSession = null;
            }
        }

        #endregion // Public Methods

        #region Private Methods

        private void OnSessionDisconnected(object sender, EventArgs e)
        {
            _logger.Info("TCP connection with {0}:{1} was closed; session will end.", (object)_activeSession.RemoteEndPoint.Address,
                (object)_activeSession.RemoteEndPoint.Port);

            // do nothing here; the session will trigger SessionEnded
        }

        private async void OnSessionReceiveAliveTimeout(object sender, EventArgs e)
        {
            _logger.Info("Receive alive timeout occured; closing session.");

            try
            {
                await _activeSession.CloseSessionAsync();
                _tokenSource?.Cancel();
            }
            catch (TaskCanceledException)
            {
                _tokenSource?.Cancel();
            }
            finally
            {
                DisposeActiveSession();
            }
        }

        private void OnSessionEnded(object sender, EventArgs e)
        {
            DisposeActiveSession();
            TLCSessionEnded?.Invoke(this, null);
        }

        #endregion // Private Methods

        #region Constructor

        public TLCFIClientSessionManager(IPEndPoint endPoint)
        {
            _endPoint = endPoint;
        }

        #endregion // Constructor
    }
}