using System.Security.Cryptography;
using System.Text;

namespace CodingConnected.TLCFI.NET.Core.Helpers
{
	/// <summary>
	/// Wrapper class around SHA256 encoding capabilities of .NET
	/// </summary>
    public static class Sha256Hasher
    {
        private static readonly SHA256 _hash;

        static Sha256Hasher()
        {
            _hash = SHA256.Create();
        }

		/// <summary>
		/// Encodes a given string using the SHA256 algorithm. This can be
		/// useful to store passwords safely.
		/// </summary>
		/// <param name="value">The string to be encoded</param>
		/// <returns>The string as a SHA256 encoded string</returns>
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