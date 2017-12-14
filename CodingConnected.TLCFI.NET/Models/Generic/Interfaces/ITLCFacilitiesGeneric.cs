namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    public interface ITLCFacilitiesGeneric : ITLCFIPeer
    {
        RegistrationReply Register(RegistrationRequest request);
        DeregistrationReply Deregister(DeregistrationRequest request);
    }
}
