using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class ListUnusedFilesOperationTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            var scriptFile = new FreeFile("", @"Custom\Scripts\MyScript.cs");
            var filteredFile = new FreeFile("", @"Saves\Filtered\FilteredScript.cs");
            var scriptListChild = new FreeFile("", @"Custom\Scripts\Complex\Child.cs");
            var scriptListFile = new FreeFile("", @"Custom\Scripts\Complex\Complex.cslist")
            {
                Children = new List<FreeFile> { scriptListChild }
            };
            var matches = new List<FreeFilePackageMatch>
            {
                new FreeFilePackageMatch(
                     new VarPackage(new VarPackageName("Author.Name.1.var"), "absolute-path", new List<VarPackageFile>()),
                     new VarPackageFile(@"Custom\Scripts\MyScript.cs", "hash"),
                     new[] { scriptFile, scriptListFile }
                )
            };
            var op = new ListUnusedFilesOperation(_consoleOutput.Object, _fs);

            var result = await op.ExecuteAsync(matches, new ExcludeFilter(new[] { @"Saves\Filtered" }));

            Assert.That(result.Select(f => f.LocalPath).OrderBy(f => f), Is.EqualTo(new[]
            {
                @"Custom\Scripts\Complex\Child.cs",
                @"Custom\Scripts\Complex\Complex.cslist",
                @"Custom\Scripts\MyScript.cs"
            }));
        }
    }
}
