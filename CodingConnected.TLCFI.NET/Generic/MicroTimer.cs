using System;
using JetBrains.Annotations;

namespace CodingConnected.TLCFI.NET.Generic
{
    /// <summary>
    /// MicroStopwatch class
    /// </summary>
    public class MicroStopwatch : System.Diagnostics.Stopwatch
    {
        readonly double _microSecPerTick =
            1000000D / Frequency;

        public MicroStopwatch()
        {
            if (!IsHighResolution)
            {
                throw new Exception("On this system the high-resolution " +
                                    "performance counter is not available");
            }
        }

        public long ElapsedMicroseconds => (long)(ElapsedTicks * _microSecPerTick);
    }

    /// <summary>
    /// MicroTimer class
    /// </summary>
    public class MicroTimer
    {
        public delegate void MicroTimerElapsedEventHandler(
            object sender,
            MicroTimerEventArgs timerEventArgs);
        [UsedImplicitly]
        public event MicroTimerElapsedEventHandler MicroTimerElapsed;

        System.Threading.Thread _threadTimer = null;
        long _ignoreEventIfLateBy = long.MaxValue;
        long _timerIntervalInMicroSec = 0;
        bool _stopTimer = true;

        public MicroTimer()
        {
        }

        public MicroTimer(long timerIntervalInMicroseconds)
        {
            Interval = timerIntervalInMicroseconds;
        }

        public long Interval
        {
            get => System.Threading.Interlocked.Read(
                ref _timerIntervalInMicroSec);
            set => System.Threading.Interlocked.Exchange(
                ref _timerIntervalInMicroSec, value);
        }

        public long IgnoreEventIfLateBy
        {
            get => System.Threading.Interlocked.Read(
                ref _ignoreEventIfLateBy);
            set => System.Threading.Interlocked.Exchange(
                ref _ignoreEventIfLateBy, value <= 0 ? long.MaxValue : value);
        }

        [UsedImplicitly]
        public bool Enabled
        {
            set
            {
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
            }
            get => _threadTimer != null && _threadTimer.IsAlive;
        }

        public void Start()
        {
            if (Enabled || Interval <= 0)
            {
                return;
            }

            _stopTimer = false;

            void ThreadStart()
            {
                NotificationTimer(ref _timerIntervalInMicroSec, ref _ignoreEventIfLateBy, ref _stopTimer);
            }

            _threadTimer = new System.Threading.Thread(ThreadStart)
            {
                Priority = System.Threading.ThreadPriority.Highest
            };
            _threadTimer.Start();
        }

        [UsedImplicitly]
        public void Stop()
        {
            _stopTimer = true;
        }

        [UsedImplicitly]
        public void StopAndWait()
        {
            StopAndWait(System.Threading.Timeout.Infinite);
        }

        [UsedImplicitly]
        public bool StopAndWait(int timeoutInMilliSec)
        {
            _stopTimer = true;

            if (!Enabled || _threadTimer.ManagedThreadId ==
                System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                return true;
            }

            return _threadTimer.Join(timeoutInMilliSec);
        }

        [UsedImplicitly]
        public void Abort()
        {
            _stopTimer = true;

            if (Enabled)
            {
                _threadTimer.Abort();
            }
        }

        void NotificationTimer(ref long timerIntervalInMicroSec,
            ref long ignoreEventIfLateBy,
            ref bool stopTimer)
        {
            var timerCount = 0;
            long nextNotification = 0;

            var microStopwatch = new MicroStopwatch();
            microStopwatch.Start();

            while (!stopTimer)
            {
                var callbackFunctionExecutionTime =
                    microStopwatch.ElapsedMicroseconds - nextNotification;

                var timerIntervalInMicroSecCurrent =
                    System.Threading.Interlocked.Read(ref timerIntervalInMicroSec);
                var ignoreEventIfLateByCurrent =
                    System.Threading.Interlocked.Read(ref ignoreEventIfLateBy);

                nextNotification += timerIntervalInMicroSecCurrent;
                timerCount++;
                long elapsedMicroseconds;

                while ((elapsedMicroseconds = microStopwatch.ElapsedMicroseconds)
                       < nextNotification)
                {
                    System.Threading.Thread.SpinWait(10);
                }

                var timerLateBy = elapsedMicroseconds - nextNotification;

                if (timerLateBy >= ignoreEventIfLateByCurrent)
                {
                    continue;
                }

                var microTimerEventArgs =
                    new MicroTimerEventArgs(timerCount,
                        elapsedMicroseconds,
                        timerLateBy,
                        callbackFunctionExecutionTime);
                MicroTimerElapsed?.Invoke(this, microTimerEventArgs);
            }

            microStopwatch.Stop();
        }
    }

    /// <summary>
    /// MicroTimer Event Argument class
    /// </summary>
    public class MicroTimerEventArgs : EventArgs
    {
        // Simple counter, number times timed event (callback function) executed
        public int TimerCount { get; private set; }

        // Time when timed event was called since timer started
        public long ElapsedMicroseconds { get; private set; }

        // How late the timer was compared to when it should have been called
        public long TimerLateBy { get; private set; }

        // Time it took to execute previous call to callback function (OnTimedEvent)
        public long CallbackFunctionExecutionTime { get; private set; }

        public MicroTimerEventArgs(int timerCount,
            long elapsedMicroseconds,
            long timerLateBy,
            long callbackFunctionExecutionTime)
        {
            TimerCount = timerCount;
            ElapsedMicroseconds = elapsedMicroseconds;
            TimerLateBy = timerLateBy;
            CallbackFunctionExecutionTime = callbackFunctionExecutionTime;
        }
    }
}
