using System;

namespace CodingConnected.TLCFI.NET.Exceptions
{
    public class TLCFISessionException : Exception
    {
        #region Properties

        public bool Fatal { get; }

        #endregion // Properties

        #region Constructor

        public TLCFISessionException(string message, bool fatal = false) : base(message)
        {
            Fatal = fatal;
        }

        #endregion // Constructor
    }
}
