namespace CodingConnected.TLCFI.NET.Models.TLC
{
    public abstract class TLCObjectBase
    {
        #region Properties

        public abstract string Id { get; set; }
        public abstract TLCObjectType ObjectType { get; }
        public abstract bool StateChanged { get; }

        #endregion // Properties

        #region Methods

        public virtual void ResetChanged() { }
        public abstract void CopyState(object o);
        public abstract object GetMeta();
        public abstract object GetState(bool tlc = false);
        public abstract object GetFullState(bool tlc = false);

        #endregion // Methods
    }
}
