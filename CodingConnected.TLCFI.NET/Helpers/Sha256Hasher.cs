using System.Security.Cryptography;
using System.Text;

namespace CodingConnected.TLCFI.NET.Helpers
{
    public static class Sha256Hasher
    {
        private static readonly SHA256 _hash;

        static Sha256Hasher()
        {
            _hash = SHA256.Create();
        }

        public static string HashIt(string value)
        {
            var sb = new StringBuilder();

            var enc = Encoding.UTF8;
            var result = _hash.ComputeHash(enc.GetBytes(value));

            foreach (var b in result)
                sb.Append(b.ToString("x2"));

            return sb.ToString();
        }
    }
}