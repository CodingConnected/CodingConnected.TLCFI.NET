using System.Collections.Generic;
using CodingConnected.TLCFI.NET.Client.Data;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.TLC;

namespace CodingConnected.TLCFI.NET.Client.Tests
{
	public static class TLCFITestHelpers
	{
		public static TLCFIClientConfig GetClientConfig(
			int port, 
			bool autoreconnect = true,
			int maxerrorcount = 5, 
			bool subscribetoalloutputs = false, 
			bool usetlcforsubsription = false)
		{
			return new TLCFIClientConfig
			{
				RemoteAddress = "127.0.0.1",
				RemotePort = port,
				Password = "secretpass",
				Username = "controluser",
				RemoteIntersectionId = "K205195",
				IveraUri = "http://127.0.0.1/",
				SignalGroupIds = new List<string>
				{
					"02", "03", "05", "08"
				},
				DetectorIds = new List<string>
				{
					"021", "022", "023", "024", "025", "026", "027", "028",
					"031", "032", "033",
					"051", "052",
					"081", "082", "083", "084", "085", "086"
				},
				InputIds = new List<string>(),
				OutputIds = new List<string>(),
				ApplicationType = ApplicationType.Control,
				MaxSessionErrorCount = maxerrorcount,
				AutoReconnect = autoreconnect,
				EndCapability = HandoverCapability.Cleared,
				StartCapability = HandoverCapability.Cleared,
				RegisterDelayAfterConnecting = 0,
				SubscribeToAllOutputs = subscribetoalloutputs,
				UseIdsFromTLCForSubscription = usetlcforsubsription
			};
		}
	}
}