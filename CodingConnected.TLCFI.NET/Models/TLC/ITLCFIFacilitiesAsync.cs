using System.Threading;
using System.Threading.Tasks;
using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public interface ITLCFIFacilitiesAsync : ITLCFacilitiesGenericAsync
    {
        Task<Generic.ObjectData> SubscribeAsync(Generic.ObjectReference objectreference, CancellationToken token);
        Task UpdateStateAsync(Generic.ObjectStateUpdateGroup objectstateupdategroup, CancellationToken token);
        Task NotifyEventAsync(Generic.ObjectEvent objectevent, CancellationToken token);
    }
}
