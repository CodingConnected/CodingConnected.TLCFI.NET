namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public enum SignalGroupState
    {
        Unavailable = 0,
        Dark = 1,
        StopThenProceed = 2,
        StopAndRemain = 3,
        PreMovement = 4,
        PermissiveMovementAllowed = 5,
        ProtectedMovementAllowed = 6,
        PermissiveClearance = 7,
        ProtectedClearance = 8,
        CautionConflictingTraffic = 9,
        PermissiveMovementPreClearance = 10,
        ProtectedMovementPreClearance = 11
    }
}
