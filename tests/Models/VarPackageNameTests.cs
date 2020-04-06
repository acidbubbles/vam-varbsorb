using NUnit.Framework;

namespace Varbsorb.Models
{
    public class VarPackageNameTests
    {
        [Test]
        public void CanParseName()
        {
            if (!VarPackageName.TryGet("Author.Package.1.var", out var name))
                Assert.Fail("Could not parse");

            Assert.That(name.Author, Is.EqualTo("Author"));
            Assert.That(name.Name, Is.EqualTo("Package"));
            Assert.That(name.Version, Is.EqualTo(1));
        }

        [Test]
        public void CanParseWildcards()
        {
            if (!VarPackageName.TryGet("*.*.*.var", out var name))
                Assert.Fail("Could not parse");

            Assert.That(name.Author, Is.EqualTo("*"));
            Assert.That(name.Name, Is.EqualTo("*"));
            Assert.That(name.Version, Is.EqualTo(-1));
        }
    }
}
