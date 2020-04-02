using NUnit.Framework;

namespace Varbsorb
{
    public class VarbsorberTests
    {
        [Test]
        public void CanCreateFilter()
        {
            var filter = Varbsorber.BuildFilter(@"C:\Vam", new[] { @"Saves\scene" }, new[] { @"Saves\scene\mine" });

            Assert.That(filter.IsFiltered(@"Saves\scene\mine\scene\scene.json"), Is.True);
            Assert.That(filter.IsFiltered(@"Saves\scene\other\scene\scene.json"), Is.False);
        }
    }
}
