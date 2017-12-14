using System;
using CodingConnected.TLCFI.NET.Core.Models.TLC;

namespace CodingConnected.TLCFI.NET.Core.Exceptions
{
    public class TLCObjectNotFoundException : Exception
    {
		#region Static methods

	    private static string GetMessage(string objectId)
	    {
		    return $"Object with Id \"{objectId}\" could not be found";
	    }

	    private static string GetMessage(string objectId, TLCObjectType type)
	    {
		    string stype;
		    switch (type)
		    {
			    case TLCObjectType.Session:
				    stype = "Session";
					break;
			    case TLCObjectType.TLCFacilities:
				    stype = "TLCFacilities";
				    break;
			    case TLCObjectType.Intersection:
				    stype = "Intersection";
				    break;
			    case TLCObjectType.SignalGroup:
				    stype = "SignalGroup";
				    break;
			    case TLCObjectType.Detector:
				    stype = "Detector";
				    break;
			    case TLCObjectType.Input:
				    stype = "Input";
				    break;
			    case TLCObjectType.Output:
				    stype = "Output";
				    break;
			    case TLCObjectType.SpecialVehicleEventGenerator:
				    stype = "SpecialVehicleEventGenerator";
				    break;
			    case TLCObjectType.Variable:
				    stype = "Variable";
				    break;
			    default:
				    throw new ArgumentOutOfRangeException(nameof(type), type, null);
		    }
		    return $"Object of type {stype} with Id \"{objectId}\" could not be found";
	    }

		#endregion // Static methods

		#region Constructor

		public TLCObjectNotFoundException(string objectId) : base(GetMessage(objectId))
	    {
		    
	    }

	    public TLCObjectNotFoundException(string objectId, TLCObjectType type) : base(GetMessage(objectId, type))
	    {

	    }

		#endregion // Constructor
	}
}