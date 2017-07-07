using System;
using Newtonsoft.Json;

namespace CodingConnected.TLCFI.NET.Models.Generic
{
    public class ProtocolVersion
    {
        private int _major;
        private int _minor;
        private int _revision;

        [JsonProperty("major")]
        public int Major
        {
            get => _major;
            set
            {
                if (value < 0 || value > 1000)
                    throw new NotImplementedException("Version major must be between 0 and 1000");
                _major = value;
            }
        }

        [JsonProperty("minor")]
        public int Minor
        {
            get => _minor;
            set
            {
                if (value < 0 || value > 1000)
                    throw new NotImplementedException("Version minor must be between 0 and 1000");
                _minor = value;
            }
        }

        [JsonProperty("revision")]
        public int Revision
        {
            get => _revision;
            set
            {
                if (value < 0 || value > 1000)
                    throw new NotImplementedException("Version revision must be between 0 and 1000");
                _revision = value;
            }
        }

        public override string ToString()
        {
            return Major + "." + Minor + "." + Revision;
        }

        #region Constructors

        public ProtocolVersion(int ma, int mi, int rev)
        {
            Major = ma;
            Minor = mi;
            Revision = rev;
        }

        public ProtocolVersion()
        {
            
        }

        #endregion // Constructors
    }
}
