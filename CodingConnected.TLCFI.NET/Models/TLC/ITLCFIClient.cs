using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public interface ITLCFIClient : ITLCFIPeer
    {
        void UpdateState(ObjectStateUpdateGroup objectstateupdategroup);
        void NotifyEvent(ObjectEvent objectevent);
    }
}
