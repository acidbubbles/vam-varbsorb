using System;
using System.Security.Cryptography;

namespace Varbsorb
{
    public interface IHashingAlgo : IDisposable
    {
        string GetHash(byte[] bytes);
    }

    public class SHA1HashingAlgo : IHashingAlgo
    {
        private readonly SHA1 _sha1 = SHA1.Create();

        public string GetHash(byte[] bytes)
        {
            var hash = _sha1.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        public void Dispose()
        {
            _sha1.Dispose();
        }
    }
}