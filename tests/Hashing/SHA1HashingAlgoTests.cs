using System.Text;
using NUnit.Framework;

namespace Varbsorb.Hashing
{
    public class SHA1HashingAlgoTests
    {
        [Test]
        public void CanHashReliably()
        {
            var sha1 = new SHA1HashingAlgo();

            var hash1 = sha1.GetHash(Encoding.UTF8.GetBytes(@"Some binary data"));
            var hash2 = sha1.GetHash(Encoding.UTF8.GetBytes(@"Some binary data"));

            Assert.That(hash1, Is.EqualTo(hash2));
        }
    }
}
