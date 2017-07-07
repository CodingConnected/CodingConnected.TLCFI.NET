namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public enum DetectorFaultState
    {
        None = 0,
        TooLongUnoccupied = 1,
        TooLongOccupied = 2,
        Flutter = 3,
        HardwareError = 4
    }
}
