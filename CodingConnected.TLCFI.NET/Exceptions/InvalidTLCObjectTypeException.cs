using System;

namespace CodingConnected.TLCFI.NET.Core.Exceptions
{
    public class InvalidTLCObjectTypeException : Exception
    {
        #region Constructor

        public InvalidTLCObjectTypeException(string message) : base(message)
        {
            
        }

        #endregion // Constructor
    }
}