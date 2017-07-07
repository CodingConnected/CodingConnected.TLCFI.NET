using System;

namespace CodingConnected.TLCFI.NET.Exceptions
{
    public class InvalidMetaReceivedException : Exception
    {
        #region Constructor

        public InvalidMetaReceivedException(string message) : base(message)
        {
            
        }

        #endregion // Constructor
    }
}