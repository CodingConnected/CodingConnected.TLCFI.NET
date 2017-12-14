using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.Generic;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public interface ITLCFIFacilities : ITLCFacilitiesGeneric
    {
        ObjectData Subscribe(ObjectReference objectreference);
        void UpdateState(ObjectStateUpdateGroup objectstateupdategroup);
        void NotifyEvent(ObjectEvent objectevent);
    }
}
