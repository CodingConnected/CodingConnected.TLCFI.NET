using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public interface ITLCFIClientAsync : ITLCFIPeerAsync
    {
        Task UpdateStateAsync(ObjectStateUpdateGroup objectstateupdategroup, CancellationToken token);
        Task NotifyEventAsync(ObjectEvent objectevent, CancellationToken token);
    }
}
