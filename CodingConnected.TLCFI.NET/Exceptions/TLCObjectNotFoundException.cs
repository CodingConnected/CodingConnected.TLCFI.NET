using System;

namespace CodingConnected.TLCFI.NET.Exceptions
{
    public class TLCObjectNotFoundException : Exception
    {
        #region Constructor

        public TLCObjectNotFoundException(string message) : base(message)
        {

        }

        #endregion // Constructor
    }
}