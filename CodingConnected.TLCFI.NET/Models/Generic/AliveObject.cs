using System;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public class AliveObject
    {
        #region Properties

        [JsonProperty("ticks")] public uint Ticks;

        [JsonProperty("time")] public ulong Time;
        
        #endregion // Properties
    }
}
