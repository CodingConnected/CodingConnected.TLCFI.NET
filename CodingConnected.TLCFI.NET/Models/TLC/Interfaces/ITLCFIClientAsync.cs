using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.Generic;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public interface ITLCFIClientAsync : ITLCFIPeerAsync
    {
        Task UpdateStateAsync(ObjectStateUpdateGroup objectstateupdategroup, CancellationToken token);
        Task NotifyEventAsync(ObjectEvent objectevent, CancellationToken token);
    }
}
