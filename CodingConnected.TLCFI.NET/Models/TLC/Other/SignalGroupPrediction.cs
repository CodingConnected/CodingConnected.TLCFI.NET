using System;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class SignalGroupPrediction
    {
        private int? _confidence;

        #region Properties

        [JsonProperty("state")]
        public SignalGroupState State { get; set; }

        [JsonProperty("startTime")]
        public uint? StartTime { get; set; }        // Ticks

        [JsonProperty("minEnd")]
        public uint MinEnd { get; set; }        // Ticks

        [JsonProperty("maxEnd")]
        public uint? MaxEnd { get; set; }       // ticks

        [JsonProperty("likelyEnd")]
        public uint? LikelyEnd { get; set; } // ticks

        [JsonProperty("confidence")]
        public int? Confidence
        {
            get => _confidence;
            set
            {
                if (value < 0 || value > 100)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _confidence = value; 
                
            }
        }

        [JsonProperty("next")]
        public uint? Next { get; set; }      // ticks
        
        #endregion // Properties
    }
}
