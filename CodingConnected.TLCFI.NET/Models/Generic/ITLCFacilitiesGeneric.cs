namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public interface ITLCFacilitiesGeneric : ITLCFIPeer
    {
        RegistrationReply Register(RegistrationRequest request);
        DeregistrationReply Deregister(DeregistrationRequest request);
    }
}
