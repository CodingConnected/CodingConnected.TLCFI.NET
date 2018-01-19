using System;

namespace CodingConnected.TLCFI.NET.Core.Tools
{
    public static class TimestampGenerator
    {
		#region Fields

		private static readonly DateTime JanFirst1970 = new DateTime(1970, 1, 1, 0, 0, 0);

		#endregion // Fields

		#region Public methods

		public static ulong GetTimestamp()
        {
            return (ulong)((DateTime.Now.ToUniversalTime() - JanFirst1970).TotalMilliseconds + 0.5);
        }
		
		#endregion // Public methods
    }
}
