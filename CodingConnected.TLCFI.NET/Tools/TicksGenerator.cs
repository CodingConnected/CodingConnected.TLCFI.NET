using System.Diagnostics;

namespace CodingConnected.TLCFI.NET.Core.Tools
{
    public class TicksGenerator : ITicksGenerator
    {
		#region Constants

		private const uint MaxTicksRange = 4294967295;

		#endregion // Constants

        #region Fields

        private readonly Stopwatch _ticksStopWatch;
        private uint _overflow;
        private static readonly object _locker = new object();
        private static volatile ITicksGenerator _default;

        #endregion // Fields

        #region Properties

        public static ITicksGenerator Default
        {
            get
            {
	            if (_default != null) return _default;
	            lock (_locker)
	            {
		            if (_default == null)
		            {
			            _default = new TicksGenerator();
		            }
	            }
	            return _default;
            }
        }

        #endregion // Properties

        #region Public Methods

        public uint GetCurrentTicks()
        {
            if(_ticksStopWatch.ElapsedMilliseconds + _overflow > MaxTicksRange)
            {
                _overflow = (uint)_ticksStopWatch.ElapsedMilliseconds + _overflow - MaxTicksRange;
                _ticksStopWatch.Restart();
            }
            return (uint)_ticksStopWatch.ElapsedMilliseconds + _overflow;
        }

	    public void OverrideDefault(ITicksGenerator ticksGenerator)
	    {
		    _default = ticksGenerator;
	    }

	    public void Reset()
	    {
		    _default = null;
	    }

        #endregion // Public Methods

        #region Constructor

	    private TicksGenerator()
        {
            _overflow = 0;

            _ticksStopWatch = new Stopwatch();
            _ticksStopWatch.Start();
        }

        #endregion // Constructor
    }
}
