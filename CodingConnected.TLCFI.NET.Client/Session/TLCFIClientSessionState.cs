using System;
using NLog;

namespace CodingConnected.TLCFI.NET.Client.Session
{
    /// <summary>
    /// Represents the session with a remote TLC Facilities.
    /// This class handled TLC-FI JSON-RPC calls and monitors session health via alive checking.
    /// The class responds to JSON-RPC calls by calling methods and setting state in the CLABase class.
    /// </summary>
    public class TLCFIClientSessionState
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private bool _registered;
        private bool _configured;
        private bool _sessionControl;
        private bool _intersectionControl;
        
        #endregion // Fields

        #region Properties

        public bool Registered
        {
            get => _registered;
            set
            {
                _registered = value;
                RegisteredChanged?.Invoke(this, value);
            }
        }

        public bool Configured
        {
            get => _configured;
            set
            {
                _configured = value;
                ConfiguredChanged?.Invoke(this, value);
            }
        }

        public bool SessionControl
        {
            get => _sessionControl;
            set
            {
                if (value != _sessionControl)
                {
                    SessionControlChanged?.Invoke(this, value);
                }
                _sessionControl = value;
            }
        }

        public bool IntersectionControl
        {
            get => _intersectionControl;
            set
            {
                if (value != _intersectionControl)
                {
                    IntersectionControlChanged?.Invoke(this, value);
                }
                _intersectionControl = value;
            }
        }

        public bool Controlling => _sessionControl && _intersectionControl;

        public bool SystemsAlive { get; set; }

        #endregion // Properties

        #region Events

        public event EventHandler<bool> RegisteredChanged;
        public event EventHandler<bool> ConfiguredChanged;
        public event EventHandler<bool> SessionControlChanged;
        public event EventHandler<bool> IntersectionControlChanged;

        #endregion // Events

        #region Constructor

        public TLCFIClientSessionState()
        {
            SystemsAlive = true;
        }

        #endregion // Constructor
    }
}
