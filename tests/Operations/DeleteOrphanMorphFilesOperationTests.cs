using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteOrphanMorphFilesOperationTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            var files = new List<FreeFile>
            {
                MockFile(@"Saves\scene\Custom\Person\female\morph1.vmb", ""),
                MockFile(@"Saves\scene\Custom\Person\female\morph1.vmi", ""),
                MockFile(@"Saves\scene\Custom\Person\male\morph1.vmi", ""),
                MockFile(@"Saves\Filtered\morph1.vmi", "")
            };
            var op = new DeleteOrphanMorphFilesOperation(_consoleOutput.Object, _fs, Mock.Of<IRecycleBin>(), Mock.Of<ILogger>());

            await op.ExecuteAsync(files, DeleteOptions.Permanent, GivenExcludes(@"Saves\Filtered"), VerbosityOptions.Default, ExecutionOptions.Default);

            Assert.That(_fs.Directory.GetFiles(_vamPath, "*.*", SearchOption.AllDirectories), Is.EquivalentTo(new[]
            {
                $@"{_vamPath}\Saves\scene\Custom\Person\female\morph1.vmb",
                $@"{_vamPath}\Saves\scene\Custom\Person\female\morph1.vmi",
                $@"{_vamPath}\Saves\Filtered\morph1.vmi",
            }));
        }
    }
}
