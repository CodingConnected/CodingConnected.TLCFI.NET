using CodingConnected.TLCFI.NET.Models.Generic;

namespace CodingConnected.TLCFI.NET.Data
{
    public interface ITLCFIDataProvider
    {
        #region Properties

        ProtocolVersion ProtocolVersion { get; }
        TLCFISettings Settings { get; }

        #endregion // Properties

        #region Methods

        string IsProtocolVersionOk(ProtocolVersion version);
        
        #endregion // Methods
    }
}
