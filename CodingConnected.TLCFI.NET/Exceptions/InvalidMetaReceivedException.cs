using System;

namespace CodingConnected.TLCFI.NET.Core.Exceptions
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