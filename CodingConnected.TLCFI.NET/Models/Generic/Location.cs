using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using CodingConnected.TLCFI.NET.Tools;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public class Location
    {
        #region Fields

        private double _latitude;
        private double _longitude;
        private double _elevation;

        #endregion // Fields

        #region Properties

        [JsonProperty("latitude")]
        public double Latitude
        {
            get => _latitude;
            set
            {
                if (value < -90.0 || value > 90.0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _latitude = value; 
            }
        }

        [JsonProperty("longitude")]
        public double Longitude
        {
            get => _longitude;
            set
            {
                if (value < -180.0 || value > 180.0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _longitude = value; 
            }
        }

        [JsonProperty("elevation")]
        public double Elevation
        {
            get => _elevation;
            set
            {
                if (value < -100.0 || value > 8000.0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                _elevation = value; 
            }
        }
        
        #endregion // Properties
    }
}
