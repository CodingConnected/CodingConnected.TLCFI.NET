using System;

namespace CodingConnected.TLCFI.NET.Core.Exceptions
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