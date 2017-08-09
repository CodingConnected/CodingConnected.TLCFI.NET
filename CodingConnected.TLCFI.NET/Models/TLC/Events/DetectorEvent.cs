using System;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class DetectorEvent
    {
        #region Fields

        private float _objectSpeed;
        private double _objectLength;
        private double? _objectHeight;
        private double? _objectWidth;

        #endregion // Fields

        #region Properties

        [JsonProperty("objectspeed")]
        public float ObjectSpeed
        {
            get => _objectSpeed;
            set
            {
                if (value > 0.0 && value <= 99.0)
                {
                    _objectSpeed = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [JsonProperty("objectlength")]
        public double ObjectLength
        {
            get => _objectLength;
            set
            {
                if (value > 0 && value < 429496729.5)
                {
                    _objectLength = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [JsonProperty("objectheight")]
        public double? ObjectHeight
        {
            get => _objectHeight;
            set
            {
                if (value > 0 && value < 429496729.5)
                {
                    _objectHeight = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [JsonProperty("objectwidth")]
        public double? ObjectWidth
        {
            get => _objectWidth;
            set
            {
                if (value > 0 && value < 429496729.5)
                {
                    _objectWidth = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        [JsonProperty("classification")]
        public DetectorClassification Classification { get; set; }

        [JsonProperty("direction")]
        public DetectorDirection Direction { get; set; }

        #endregion // Properties

        #region Overrides

        public override string ToString()
        {
            return "DetectorEvent: speed=" + ObjectSpeed + 
                   " length=" + ObjectLength + 
                   " height=" + ObjectHeight +
                   " width=" + ObjectWidth + 
                   " classification=" + Classification + 
                   " direction=" + Direction;
        }

        #endregion // Overrides
    }
}
