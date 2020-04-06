using NUnit.Framework;
using Varbsorb.Models;

namespace Varbsorb

{
    public class FilterTests
    {
        [TestCase(null, null, @"Saves\folder\file.json", true)]
        [TestCase(new[] { @"Saves\folder" }, null, @"Saves\folder\file.json", true)]
        [TestCase(null, new[] { @"Saves\folder" }, @"Saves\folder\file.json", false)]
        [TestCase(new[] { @"Saves\folder" }, new[] { @"Saves\folder\file.json" }, @"Saves\folder\file.json", false)]
        public void PathFilter(string[] includes, string[] excludes, string path, bool included)
        {
            var filter = Filter.From(@"C:\VaM", includes, excludes);
            Assert.That(!filter.IsFiltered(path), Is.EqualTo(included));
        }

        [TestCase(null, null, @"Author.Package.1.var", true)]
        [TestCase(new[] { @"Author.*.*.var" }, null, @"Author.Package.1.var", true)]
        [TestCase(null, new[] { @"Author.*.*.var" }, @"Author.Package.1.var", false)]
        [TestCase(new[] { @"Author.*.*.var" }, new[] { @"Author.Package.1.var" }, @"Author.Package.1.var", false)]
        public void PackageFilter(string[] includes, string[] excludes, string name, bool included)
        {
            var filter = Filter.From(@"C:\VaM", includes, excludes);
            Assert.That(!filter.IsFiltered(VarPackageName.TryGet(name, out var x) ? x : null), Is.EqualTo(included));
        }
    }
}
