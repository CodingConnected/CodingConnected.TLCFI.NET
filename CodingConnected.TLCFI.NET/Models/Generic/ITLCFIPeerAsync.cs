using System.Threading;
using System.Threading.Tasks;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public interface ITLCFIPeerAsync
    {
        Task<AliveObject> AliveAsync(AliveObject alive, CancellationToken token);
        Task<ObjectMeta> ReadMetaAsync(ObjectReference objectReference, CancellationToken token);
    }
}
