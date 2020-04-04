using System.IO.Abstractions.TestingHelpers;
using Moq;
using NUnit.Framework;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class VarbsorberTests
    {
        [Test]
        public void CanCreateFilter()
        {
            var varbsorber = new Varbsorber(Mock.Of<IConsoleOutput>(), new MockFileSystem(), Mock.Of<IOperationsFactory>());
            var filter = varbsorber.BuildFilter(@"C:\Vam", new[] { @"Saves\scene" }, new[] { @"Saves\scene\mine" });

            Assert.That(filter.IsFiltered(@"Saves\scene\mine\scene\scene.json"), Is.True);
            Assert.That(filter.IsFiltered(@"Saves\scene\other\scene\scene.json"), Is.False);
        }
    }
}
