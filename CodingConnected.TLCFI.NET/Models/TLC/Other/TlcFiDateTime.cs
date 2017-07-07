using CodingConnected.TLCFI.NET.Tools;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class TlcFiDateTime
    {
        private int _year;
        private int _month;
        private int _day;
        private int _hours;
        private int _minutes;
        private int _seconds;
        private int _miliseconds;

        #region Properties

        [JsonProperty("y")]
        public int Year
        {
            get => _year;
            set
            {
                ValueChecker.CheckValidYear(value);
                _year = value; 
            }
        }

        [JsonProperty("m")]
        public int Month
        {
            get => _month;
            set
            {
                ValueChecker.CheckValidMonth(value);
                _month = value;
            }
        }

        [JsonProperty("d")]
        public int Day
        {
            get => _day;
            set
            {
                ValueChecker.CheckValidDay(value);
                _day = value; 
            }
        }

        [JsonProperty("h")]
        public int Hours
        {
            get => _hours;
            set
            {
                ValueChecker.CheckValidHours(value);
                _hours = value; 
            }
        }

        [JsonProperty("min")]
        public int Minutes
        {
            get => _minutes;
            set
            {
                ValueChecker.CheckValidMinutes(value);
                _minutes = value; 
            }
        }

        [JsonProperty("s")]
        public int Seconds
        {
            get => _seconds;
            set
            {
                ValueChecker.CheckValidSeconds(value);
                _seconds = value; 
            }
        }

        [JsonProperty("ms")]
        public int Miliseconds
        {
            get => _miliseconds;
            set
            {
                ValueChecker.CheckValidMiliseconds(value);
                _miliseconds = value;     
            }
        }

        #endregion // Properties
    }
}