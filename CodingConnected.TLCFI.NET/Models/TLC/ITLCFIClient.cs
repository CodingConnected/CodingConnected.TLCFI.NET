using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public interface ITLCFIClient : ITLCFIPeer
    {
        void UpdateState(Generic.ObjectStateUpdateGroup objectstateupdategroup);
        void NotifyEvent(Generic.ObjectEvent objectevent);
    }
}
