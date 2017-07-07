namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public enum ControlState
    {
        Error = 0,
        NotConfigured = 1,
        Offline = 2,
        ReadyToControl = 3,
        StartControl = 4,
        InControl = 5,
        EndControl = 6
    }
}
