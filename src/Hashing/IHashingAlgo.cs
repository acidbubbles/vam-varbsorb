using System;

namespace Varbsorb.Hashing
{
    public interface IHashingAlgo
    {
        string GetHash(byte[] bytes);
    }
}
