using System;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public class SignalTiming
    {
        private int? _min;
        private int? _max;

        #region Properties

        [JsonProperty("state")]
        public SignalGroupState State { get; set; }

        [JsonProperty("min")]
        public int? Min
        {
            get => _min;
            set
            {
                if (value < 0 || value > ushort.MaxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _min = value;
            }
        }

        [JsonProperty("max")]
        public int? Max
        {
            get => _max;
            set
            {
                if (value < 0 || value > ushort.MaxValue)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _max = value; 
            }
        }

        #endregion // Properties
    }
}
