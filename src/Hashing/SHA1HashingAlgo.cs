using System;
using System.Security.Cryptography;

namespace Varbsorb.Hashing
{
    public class SHA1HashingAlgo : IHashingAlgo
    {
        public string GetHash(byte[] bytes)
        {
            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
