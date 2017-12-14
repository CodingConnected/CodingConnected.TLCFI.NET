using System;
using System.Linq;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.TLC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodingConnected.TLCFI.NET.Core.Models.Converters
{
    public class TlcObjectJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ObjectMeta) ||
                   objectType == typeof(ObjectStateUpdate) ||
                   objectType == typeof(ObjectData);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    existingValue = existingValue ?? serializer.ContractResolver.ResolveContract(objectType).DefaultCreator();
                    serializer.Populate(reader, existingValue);
                    switch (existingValue)
                    {
	                    case ObjectMeta om:
		                    var nom = new ObjectMeta
		                    {
			                    Objects = om.Objects,
			                    Meta = om.Meta.Select(mt => ((JObject) mt).ToObject(GetOJtype(om.Objects.Type))).ToArray(),
			                    Ticks = om.Ticks
		                    };
		                    return nom;
	                    case ObjectStateUpdate osu:
		                    var nosu = new ObjectStateUpdate
		                    {
			                    Objects = osu.Objects,
			                    States = osu.States.Select(mt => ((JObject) mt).ToObject(GetOJtype(osu.Objects.Type)))
				                    .ToArray()
		                    };
		                    return nosu;
	                    case ObjectData oda:
	                    {
		                    var noda = new ObjectData
		                    {
			                    Objects = oda.Objects,
			                    Data = oda.Data.Select(mt => ((JObject)mt).ToObject(GetOJtype(oda.Objects.Type)))
				                    .ToArray(),
			                    Ticks = oda.Ticks
		                    };
		                    return noda;
	                    }
	                    case ObjectEvent oev:
	                    {
		                    var noda = new ObjectEvent
		                    {
			                    Objects = oev.Objects,
			                    Events = oev.Events.Select(mt => ((JObject)mt).ToObject(GetOJtype(oev.Objects.Type)))
				                    .ToArray(),
			                    Ticks = oev.Ticks
		                    };
		                    return noda;
	                    }
                    }
	                return null;
                case JsonToken.Null:
                    return null;
                default:
                    throw new JsonSerializationException();
            }
        }

        private Type GetOJtype(TLCObjectType t)
        {
            switch (t)
            {
                case TLCObjectType.Session:
                    return typeof(ControlApplication);
                case TLCObjectType.TLCFacilities:
                    return typeof(TLCFacilities);
                case TLCObjectType.Intersection:
                    return typeof(Intersection);
                case TLCObjectType.SignalGroup:
                    return typeof(SignalGroup);
                case TLCObjectType.Detector:
                    return typeof(Detector);
                case TLCObjectType.Input:
                    return typeof(Input);
                case TLCObjectType.Output:
                    return typeof(Output);
                case TLCObjectType.SpecialVehicleEventGenerator:
                    return typeof(SpecialVehicleEventGenerator);
                case TLCObjectType.Variable:
                    return typeof(Variable);
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}