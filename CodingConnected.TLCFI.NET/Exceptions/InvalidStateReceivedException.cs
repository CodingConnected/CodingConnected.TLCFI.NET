using System;

namespace CodingConnected.TLCFI.NET.Exceptions
{
    public class InvalidStateReceivedException : Exception
    {
        #region Constructor

        public InvalidStateReceivedException(string message) : base(message)
        {
            
        }

        #endregion // Constructor
    }
}