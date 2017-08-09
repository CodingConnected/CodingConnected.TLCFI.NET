namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public interface ITLCFIPeer
    {
        AliveObject Alive(AliveObject alive);
        ObjectMeta ReadMeta(ObjectReference objectReference);
    }
}
