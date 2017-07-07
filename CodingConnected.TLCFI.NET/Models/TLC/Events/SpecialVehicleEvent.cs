using System;
using System.Text;
using CodingConnected.TLCFI.NET.Tools;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public class SpecialVehicleEvent
    {
        #region Fields

        private double? _length;
        private double? _speed;
        private int? _activationPointNr;
        private int? _companyNr;
        private int? _virtualLoop;
        private int? _lineNr;
        private int? _serviceNr;
        private int? _vehId;
        private int? _punctualityTime;
        private int? _distToStopLine;
        private int? _timeToStopLine;
        private int? _journeyNr;
        private int? _routePt;
        private int? _directionSg;
        private int? _reserve23;
        private int? _reserve24;

        #endregion // Fields

        #region Properties

        [JsonProperty("virtualLoop")]
        public int? VirtualLoop
        {
            get => _virtualLoop;
            set
            {
                ValueChecker.CheckValidVirtualLoop(value);
                _virtualLoop = value; 
            }
        }

        [JsonProperty("vehType")]
        public VehicleType? VehType { get; set; }

        [JsonProperty("lineNr")]
        public int? LineNr
        {
            get => _lineNr;
            set
            {
                ValueChecker.CheckValidVehicleType(value);
                _lineNr = value; 
            }
        }

        [JsonProperty("serviceNr")]
        public int? ServiceNr
        {
            get => _serviceNr;
            set
            {
                ValueChecker.CheckValidRouteServiceNumber(value);
                _serviceNr = value; 
            }
        }

        [JsonProperty("companyNr")]
        public int? CompanyNr
        {
            get => _companyNr;
            set
            {
                ValueChecker.CheckValidCompanyNumber(value);
                _companyNr = value; 
            }
        }

        [JsonProperty("vehId")]
        public int? VehId
        {
            get => _vehId;
            set
            {
                ValueChecker.CheckValidVehicleId(value);
                _vehId = value; 
            }
        }

        [JsonProperty("directionSG")]
        public int? DirectionSg
        {
            get => _directionSg;
            set 
            {
                ValueChecker.CheckValidDirectionSg(value);
                _directionSg = value;
            }
        }

        [JsonProperty("status")]
        public VehicleStatus? Status { get; set; }

        [JsonProperty("priorityClass")]
        public PriorityClass? PriorityClass { get; set; }

        [JsonProperty("punctualityClass")]
        public PunctualityClass? Punctuality { get; set; }

        [JsonProperty("punctualityTime")]
        public int? PunctualityTime
        {
            get => _punctualityTime;
            set
            {
                ValueChecker.CheckValidPunctualityTime(value);
                _punctualityTime = value; 
            }
        }

        [JsonProperty("length")]
        public double? Length
        {
            get => _length;
            set
            {
                ValueChecker.CheckValidLength(value);
                _length = value; 
            }
        }

        [JsonProperty("speed")]
        public double? Speed
        {
            get => _speed;
            set
            {
                ValueChecker.CheckValidSpeed(value);
                _speed = value; 
            }
        }

        [JsonProperty("distToStopLine")]
        public int? DistToStopLine
        {
            get => _distToStopLine;
            set
            {
                ValueChecker.CheckValidDistanceToStopLine(value);
                _distToStopLine = value; 
            }
        }

        [JsonProperty("timeToStopLine")]
        public int? TimeToStopLine
        {
            get => _timeToStopLine;
            set
            {
                ValueChecker.CheckValidTimeToStopLine(value);
                _timeToStopLine = value; 
            }
        }

        [JsonProperty("journeyNr")]
        public int? JourneyNr
        {
            get => _journeyNr;
            set
            {
                ValueChecker.CheckValidJourneyNumber(value);
                _journeyNr = value; 
            }
        }

        [JsonProperty("journeyCat")]
        public JourneyCategory? JourneyCat { get; set; }

        [JsonProperty("routePT")]
        public int? RoutePt
        {
            get => _routePt;
            set
            {
                ValueChecker.CheckValidRoutePublicTransport(value);
                _routePt = value; 
            }
        }

        [JsonProperty("type")]
        public AnouncementType? Type { get; set; }

        [JsonProperty("activationPointNr")]
        public int? ActivationPointNr
        {
            get => _activationPointNr;
            set
            {
                ValueChecker.CheckActivationPointNr(value);
                _activationPointNr = value; 
            }
        }

        [JsonProperty("location")]
        public Generic.Location Location { get; set; }

        [JsonProperty("dateTime")]
        public TlcFiDateTime TlcFiDateTime { get; set; }

        [JsonProperty("reserve23")]
        public int? Reserve23
        {
            get => _reserve23;
            set
            {
                ValueChecker.CheckValidSpvehSpare(value);
                _reserve23 = value; 
            }
        }

        [JsonProperty("reserve24")]
        public int? Reserve24
        {
            get => _reserve24;
            set
            {
                ValueChecker.CheckValidSpvehSpare(value);
                _reserve24 = value; 
            }
        }

        #endregion // Properties

        #region Overrides

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("SpecialVehicleEvent:");
            if (VirtualLoop.HasValue) sb.Append(" VirtualLoop=" + VirtualLoop);
            if (VehType.HasValue) sb.Append(" VehType=" + VehType);
            if (LineNr.HasValue) sb.Append(" LineNr=" + LineNr);
            if (ServiceNr.HasValue) sb.Append(" ServiceNr=" + ServiceNr);
            if (CompanyNr.HasValue) sb.Append(" CompanyNr=" + CompanyNr);
            if (VehId.HasValue) sb.Append(" VehId=" + VehId);
            if (DirectionSg.HasValue) sb.Append(" DirectionSg=" + DirectionSg);
            if (Status.HasValue) sb.Append(" Status=" + Status);
            if (PriorityClass.HasValue) sb.Append(" PriorityClass=" + PriorityClass);
            if (Punctuality.HasValue) sb.Append(" Punctuality=" + Punctuality);
            if (PunctualityTime.HasValue) sb.Append(" PunctualityTime=" + PunctualityTime);
            if (Length.HasValue) sb.Append(" Length=" + Length);
            if (Speed.HasValue) sb.Append(" Speed=" + Speed);
            if (DistToStopLine.HasValue) sb.Append(" DistToStopLine=" + DistToStopLine);
            if (TimeToStopLine.HasValue) sb.Append(" TimeToStopLine=" + TimeToStopLine);
            if (JourneyNr.HasValue) sb.Append(" JourneyNr=" + JourneyNr);
            if (JourneyCat.HasValue) sb.Append(" JourneyCat=" + JourneyCat);
            if (RoutePt.HasValue) sb.Append(" RoutePt=" + RoutePt);
            if (Type.HasValue) sb.Append(" Type=" + Type);
            if (ActivationPointNr.HasValue) sb.Append(" ActivationPointNr=" + ActivationPointNr);
            if (Location != null) sb.Append(" Location=" + Location);
            if (TlcFiDateTime != null) sb.Append(" TlcFiDateTime=" + TlcFiDateTime);
            if (Reserve23.HasValue) sb.Append(" Reserve23=" + Reserve23);
            if (Reserve24.HasValue) sb.Append(" Reserve24=" + Reserve24);
            return sb.ToString();
        }

        #endregion // Overrides
    }
}
