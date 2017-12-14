using CodingConnected.TLCFI.NET.Core.Models.Generic;
using CodingConnected.TLCFI.NET.Core.Models.Generic;

namespace CodingConnected.TLCFI.NET.Core.Models.TLC
{
    public interface ITLCFIClient : ITLCFIPeer
    {
        void UpdateState(ObjectStateUpdateGroup objectstateupdategroup);
        void NotifyEvent(ObjectEvent objectevent);
    }
}
