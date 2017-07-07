namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public enum SessionEventCode
    {
        // Generic
        Deregistered = 0,
        FacilitiesStopping = 1,

        // TLC
        UpdateStateFailedIncorrectControlState = 1000,
        UpdateStateFailedIncorrectApplicationType = 1001,
        UpdateStateFailedIncorrectIntersection = 1002
    }
}
