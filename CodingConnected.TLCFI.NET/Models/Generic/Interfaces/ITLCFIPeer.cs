namespace CodingConnected.TLCFI.NET.Core.Models.Generic
{
    public interface ITLCFIPeer
    {
        AliveObject Alive(AliveObject alive);
        ObjectMeta ReadMeta(ObjectReference objectReference);
    }
}
