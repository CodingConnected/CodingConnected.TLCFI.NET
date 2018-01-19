namespace CodingConnected.TLCFI.NET.Core.Tools
{
    public interface ITicksGenerator
    {
        uint GetCurrentTicks();
        void Reset();
        void OverrideDefault(ITicksGenerator ticksGenerator);
	}
}