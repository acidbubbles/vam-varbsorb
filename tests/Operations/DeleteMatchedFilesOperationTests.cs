using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteMatchedFilesOperationTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            var scriptFile = MockFile(@"Custom\Scripts\MyScript.cs", "...");
            var filteredFile = MockFile(@"Saves\Filtered\FilteredScript.cs", "...");
            var scriptListChild = MockFile(@"Custom\Scripts\Complex\Child.cs", "...");
            var scriptListFile = MockFile(@"Custom\Scripts\Complex\Complex.cslist", "...");
            scriptListFile.Children = new List<FreeFile> { scriptListChild };
            var files = new List<FreeFile> { scriptFile, filteredFile, scriptListFile };
            var matches = new List<FreeFilePackageMatch>
            {
                new FreeFilePackageMatch(
                     new VarPackage(new VarPackageName("Author.Name.1.var"), "absolute-path", new List<VarPackageFile>()),
                     new VarPackageFile(@"Custom\Scripts\MyScript.cs", "hash"),
                     new[] { scriptFile, scriptListFile }
                )
            };
            var op = new DeleteMatchedFilesOperation(_consoleOutput.Object, _fs, Mock.Of<IRecycleBin>(), Mock.Of<ILogger>());

            await op.ExecuteAsync(files, matches, DeleteOptions.Permanent, new ExcludeFilter(new[] { @"Saves\Filtered" }), VerbosityOptions.Default, ExecutionOptions.Default);

            Assert.That(!_fs.FileExists(scriptFile.Path));
            Assert.That(!_fs.FileExists(scriptListFile.Path));
            Assert.That(!_fs.FileExists(scriptListChild.Path));
            Assert.That(_fs.FileExists(filteredFile.Path));
            Assert.That(files, Is.EquivalentTo(new[] { filteredFile }));
        }
    }
}
