using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CodingConnected.TLCFI.NET.Core.Tools
{
	/// <summary>
	/// ValueChecked contains methods to validate the format of a
	/// variety of data as described in the TLC-FI specifications
	/// </summary>
    public static class ValueChecker
    {
        private static readonly Regex RejectedRegex = new Regex(@"[^_\-a-zA-Z0-9]");

        public static void CheckValidObjectId(string id)
        {
            if (id != null && RejectedRegex.IsMatch(id))
                throw new FormatException($"ObjectID {id} contains unallowed characters.");
        }

        public static void CheckValidObjectId(IEnumerable<string> ids)
        {
	        var enumerable = ids as string[] ?? ids.ToArray();
	        if (enumerable.Any(id => id != null && RejectedRegex.IsMatch(id)))
            {
                throw new FormatException($"ObjectID {enumerable.First(id => id != null && RejectedRegex.IsMatch(id))} contains unallowed characters.");
            }
        }

        public static void CheckValidCompanyName(string name)
        {
            if (name.Length > 32 || name.Select(c => (int) c).Any(ic => ic < 32 || ic > 126 || ic == 34 || ic == 44))
            {
                throw new FormatException($"CompanyName {name} contains unallowed characters.");
            }
        }

        public static void CheckValidFacilitiesVersion(string ver)
        {
            if (ver.Length > 32 || ver.Select(c => (int)c).Any(ic => ic < 32 || ic > 126 || ic == 34 || ic == 44))
            {
                throw new FormatException($"Version string {ver} contains unallowed characters.");
            }
        }

        public static void CheckValidApplicationString(string val)
        {
            if (val.Length > 32 || val.Select(c => (int)c).Any(ic => ic < 32 || ic > 126 || ic == 34 || ic == 44))
            {
                throw new FormatException($"Application string {val} contains unallowed characters.");
            }
        }

        public static void CheckValidVariableState(int? val)
        {
            if (val == null) return;
            if (val < short.MinValue || val > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidVariableLifetime(int? val)
        {
            if (val == null) return;
            if (val < 0 || val > 100)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidLength(double? val)
        {
            if (val == null) return;
            if (val < 0 || val > 429496729.5)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidSpeed(double? val)
        {
            if (val == null) return;
            if (val < 0 || val > 99.0)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckActivationPointNr(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidCompanyNumber(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 255)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidYear(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 9999)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidMonth(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 12)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidDay(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 255)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidHours(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 23)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidMinutes(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 59)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidSeconds(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 59)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidMiliseconds(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 999)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidDirectionSg(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 255)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidDistanceToStopLine(int? val)
        {
            if(val == null) return;
            if (val < -99 || val > 9999)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidJourneyCategory(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 99)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidJourneyNumber(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 9999)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidLineNumber(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 9999)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidPunctualityTime(int? val)
        {
            if(val == null) return;
            if (val < -3600 || val > 3600)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidRoutePublicTransport(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 99)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidRouteServiceNumber(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 9999)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidSpvehSpare(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidTimeToStopLine(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 255)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidVehicleId(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > short.MaxValue)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidVehicleType(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 99)
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public static void CheckValidVirtualLoop(int? val)
        {
            if(val == null) return;
            if (val < 0 || val > 127)
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
}
