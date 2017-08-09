using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public interface ITLCFIFacilities : ITLCFacilitiesGeneric
    {
        ObjectData Subscribe(ObjectReference objectreference);
        void UpdateState(ObjectStateUpdateGroup objectstateupdategroup);
        void NotifyEvent(ObjectEvent objectevent);
    }
}
