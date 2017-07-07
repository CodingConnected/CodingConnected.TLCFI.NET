using System.Diagnostics;

namespace CodingConnected.TLCFI.NET.Tools
{
    public class TicksGenerator : ITicksGenerator
    {
        #region Fields

        private readonly Stopwatch _ticksStopWatch;
        private readonly uint _maxTicksRange;
        private uint _overflow;
        private static readonly object _locker = new object();
        private static ITicksGenerator _default;

        #endregion // Fields

        #region Properties

        public static ITicksGenerator Default
        {
            get
            {
                if (_default == null)
                {
                    lock (_locker)
                    {
                        if (_default == null)
                        {
                            _default = new TicksGenerator();
                        }
                    }
                }
                return _default;
            }
        }

        #endregion // Properties

        #region Public Methods

        public uint GetCurrentTicks()
        {
            if(_ticksStopWatch.ElapsedMilliseconds + _overflow > _maxTicksRange)
            {
                _overflow = (uint)_ticksStopWatch.ElapsedMilliseconds + _overflow - _maxTicksRange;
                _ticksStopWatch.Restart();
            }
            return (uint)_ticksStopWatch.ElapsedMilliseconds + _overflow;
        }

        #endregion // Public Methods

        #region Constructor

        public TicksGenerator()
        {
            _overflow = 0;
            _maxTicksRange = 4294967295;

            _ticksStopWatch = new Stopwatch();
            _ticksStopWatch.Start();
        }

        #endregion // Constructor
    }
}
