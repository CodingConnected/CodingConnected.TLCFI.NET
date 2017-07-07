using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public interface ITLCFIFacilities : ITLCFacilitiesGeneric
    {
        Generic.ObjectData Subscribe(Generic.ObjectReference objectreference);
        void UpdateState(Generic.ObjectStateUpdateGroup objectstateupdategroup);
        void NotifyEvent(Generic.ObjectEvent objectevent);
    }
}
