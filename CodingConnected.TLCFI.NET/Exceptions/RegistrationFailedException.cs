using System;

namespace CodingConnected.TLCFI.NET.Exceptions
{
    public class RegistrationFailedException : Exception
    {
        #region Constructor

        public RegistrationFailedException(string message) : base(message)
        {
            
        }

        #endregion // Constructor
    }
}