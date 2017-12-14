using System.Threading;
using System.Threading.Tasks;

namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    public interface ITLCFacilitiesGenericAsync : ITLCFIPeerAsync
    {
        Task<RegistrationReply> RegisterAsync(RegistrationRequest request, CancellationToken token);
        Task<DeregistrationReply> DeregisterAsync(DeregistrationRequest request, CancellationToken token);
    }
}
