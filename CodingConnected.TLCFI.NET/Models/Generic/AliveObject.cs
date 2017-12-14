using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    public class AliveObject
    {
        #region Properties

        [JsonProperty("ticks")] public uint Ticks;

        [JsonProperty("time")] public ulong Time;
        
        #endregion // Properties
    }
}
