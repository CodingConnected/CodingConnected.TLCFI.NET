using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CodingConnected.TLCFI.NET.Models.Generic;
using NLog;
using CodingConnected.TLCFI.NET.Helpers;

namespace CodingConnected.TLCFI.NET.Data
{
    public class TLCFIDataProvider : ITLCFIDataProvider
    {
        #region Fields

        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly object _locker = new object();
        private static volatile ITLCFIDataProvider _default;

        #endregion // Fields

        #region Properties

        public TLCFISettings Settings { get; }
        public ProtocolVersion ProtocolVersion { get; }

        public static ITLCFIDataProvider Default
        {
            get
            {
                if (_default != null) return _default;
                lock (_locker)
                {
                    if (_default == null)
                    {
                        _default = new TLCFIDataProvider();
                    }
                }
                return _default;
            }
        }

        #endregion // Properties

        #region ITLCDataProvider
        
        public string IsProtocolVersionOk(ProtocolVersion version)
        {
            if (version.Major != ProtocolVersion.Major || version.Minor != ProtocolVersion.Minor)
            {
                return $"This server supports version {ProtocolVersion.Major}.{ProtocolVersion.Minor}.x of the TLC-FI";
            }
            return null;
        }
        
        #endregion // IProcedureBinder

        #region Constructor

        public TLCFIDataProvider()
        {
            ProtocolVersion = new ProtocolVersion(1, 1, 0);
            
            var settingsfile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tlcfi_settings.xml");
            try
            {
                var serializer = new XmlSerializer(typeof(TLCFISettings));
                if (File.Exists(settingsfile))
                {
                    using (var stream = File.OpenRead(settingsfile))
                    {
                        Settings = (TLCFISettings)serializer.Deserialize(stream);
                    }
                }
                else
                {
                    throw new FileNotFoundException("No settings found for TLC", settingsfile);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        #endregion // Constructor
    }
}
