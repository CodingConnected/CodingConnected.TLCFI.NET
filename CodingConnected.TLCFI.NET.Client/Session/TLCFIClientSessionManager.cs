using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Models.Generic;
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
        public event EventHandler<TLCFIClientSession> TLCSessionStarted;
        [UsedImplicitly]
        public event EventHandler<bool> TLCSessionEnded;
        [UsedImplicitly]
        public event EventHandler<ObjectEvent> TLCSessionEventOccured;

        #endregion // Events

        #region Public Methods

        public async Task<TLCFIClientSession> GetNewSession(TLCFIClientStateManager stateManager, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return null;

            lock (_locker)
            {
                if (_activeSession != null && !_activeSession.Connected)
                {
                    _logger.Warn("A session with {0}:{1} already exists, but is not connected. It will be disposed of.",
                        _activeSession.RemoteEndPoint.Address, _activeSession.RemoteEndPoint.Port);
                    DisposeActiveSession();
                    return null;
                }
                if (_activeSession != null)
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
                _logger.Info("Need 42 seconds between succesful register calls. Still need to wait {0} seconds. ",
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
            session.ControlStateSetToError += OnSessionControlStateSetToError;
            session.ReceiveAliveTimeoutOccured += OnSessionReceiveAliveTimeout;
            session.EventOccured += OnSessionEventOccured;

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
            TLCSessionStarted?.Invoke(this, session);

            return session;
        }

        public async Task EndActiveSessionAsync(bool expected)
        {
            var closeSessionAsync = _activeSession?.CloseSessionAsync(expected);
            if (closeSessionAsync != null) await closeSessionAsync;
            DisposeActiveSession();
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

                _tokenSource?.Cancel();

                _activeSession.SessionEnded -= OnSessionEnded;
                _activeSession.Disconnected -= OnSessionDisconnected;
                _activeSession.ReceiveAliveTimeoutOccured -= OnSessionReceiveAliveTimeout;
                _activeSession.ControlStateSetToError -= OnSessionControlStateSetToError;
                _activeSession.EventOccured -= OnSessionEventOccured;
                _activeSession.DisposeSession();

                _logger.Info("Session with {0}:{1} ended, closed and disposed.",
                    (object) _activeSession.RemoteEndPoint.Address,
                    (object) _activeSession.RemoteEndPoint.Port);

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
                await _activeSession.CloseSessionAsync(false);
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

        private async void OnSessionControlStateSetToError(object sender, EventArgs e)
        {
            _logger.Info("Control state set to error; closing session.");

            try
            {
                await _activeSession.CloseSessionAsync(false);
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

        private void OnSessionEnded(object sender, bool expected)
        {
            DisposeActiveSession();
            TLCSessionEnded?.Invoke(this, expected);
        }

        private void OnSessionEventOccured(object sender, ObjectEvent objectEvent)
        {
            TLCSessionEventOccured?.Invoke(this, objectEvent);
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