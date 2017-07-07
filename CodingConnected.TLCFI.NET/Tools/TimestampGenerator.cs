using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodingConnected.TLCFI.NET.Tools
{
    public static class TimestampGenerator
    {
        private static DateTime JanFirst1970 = new DateTime(1970, 1, 1, 0, 0, 0);
        public static UInt64 GetTimestamp()
        {
            return (UInt64)((DateTime.Now.ToUniversalTime() - JanFirst1970).TotalMilliseconds + 0.5);
        }
    }
}
