using System;

namespace Varbsorb.Hashing
{
    public interface IHashingAlgo : IDisposable
    {
        string GetHash(byte[] bytes);
    }
}
