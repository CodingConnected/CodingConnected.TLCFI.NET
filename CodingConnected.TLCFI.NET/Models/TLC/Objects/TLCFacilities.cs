using System;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using CodingConnected.TLCFI.NET.Core.Models.TLC.Base;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public class TLCFacilities : TLCObjectBase
    {
        #region Properties

        // NON FI

        [JsonIgnore]
        public override TLCObjectType ObjectType => TLCObjectType.TLCFacilities;

        [JsonIgnore]
        public override bool StateChanged => false;

        // META

        private string _id;
        [JsonProperty("id")]
        public override string Id
        {
            get => _id;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _id = value;
            }
        }
        private string[] _intersections;
        [JsonProperty("intersections")]
        public string [] Intersections
        {
            get => _intersections;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _intersections = value;
            }
        }
        private string[] _signalgroups;
        [JsonProperty("signalgroups")]
        public string [] Signalgroups
        {
            get => _signalgroups;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _signalgroups = value;
            }
        }
        private string[] _detectors;
        [JsonProperty("detectors")]
        public string [] Detectors
        {
            get => _detectors;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _detectors = value;
            }
        }
        private string[] _inputs;
        [JsonProperty("inputs")]
        public string [] Inputs
        {
            get => _inputs;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _inputs = value;
            }
        }
        private string[] _outputs;
        [JsonProperty("outputs")]
        public string [] Outputs
        {
            get => _outputs;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _outputs = value;
            }
        }
        private string _spvehgenerator;
        [JsonProperty("spvehgenerator")]
        public string Spvehgenerator
        {
            get => _spvehgenerator;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _spvehgenerator = value;
            }
        }
        private string[] _variables;
        [JsonProperty("variables")]
        public string [] Variables
        {
            get => _variables;
            set
            {
                Tools.ValueChecker.CheckValidObjectId(value);
                _variables = value;
            }
        }
        [JsonProperty("info")]
        public FacilitiesInformation Info { get; set; }

        #endregion // Properties

        #region TLCObjectBase Methods

        public override object GetMeta()
        {
            return new
            {
                id = Id,
                intersections = Intersections,
                signalgroups = Signalgroups,
                detectors = Detectors,
                inputs = Inputs,
                outputs = Outputs,
                spvehgenerator = Spvehgenerator,
                variables = Variables,
                info = Info
            };
        }

        public override void CopyState(object o)
        {
            throw new UnauthorizedAccessException();
        }

        public override object GetState(bool tlc = false)
        {
            throw new UnauthorizedAccessException();
        }

        public override object GetFullState(bool tlc = false)
        {
            throw new UnauthorizedAccessException();
        }

        #endregion // TLCObjectBase Methods
    }
}
