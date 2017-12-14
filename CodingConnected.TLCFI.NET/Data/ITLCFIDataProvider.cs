using CodingConnected.TLCFI.NET.Core.Models.Generic;

namespace CodingConnected.TLCFI.NET.Core.Data
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
