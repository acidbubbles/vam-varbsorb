using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Varbsorb.Operations
{
    public class ListFilesOperationsTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\Author\Scene.json", new MockFileData(""));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Script.cs", new MockFileData(""));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Package\Plugin.cslist", new MockFileData("src/Child1.cs\r\nCustom\\Scripts\\Author\\Package\\src\\Child2.cs\r\n"));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Package\src\Child1.cs", new MockFileData("child script 1"));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Package\src\Child2.cs", new MockFileData("child script 2"));
            var op = new ListFilesOperation(_consoleOutput.Object, _fs);

            var files = await op.ExecuteAsync(_vamPath);

            Assert.That(files.Select(f => f.Path).OrderBy(f => f), Is.EqualTo(new[]{
                @$"{_vamPath}\Custom\Scripts\Author\Package\Plugin.cslist",
                @$"{_vamPath}\Custom\Scripts\Author\Script.cs",
                @$"{_vamPath}\Saves\Author\Scene.json",
            }));

            Assert.That(files.Select(f => f.LocalPath).OrderBy(f => f), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Package\Plugin.cslist",
                @"Custom\Scripts\Author\Script.cs",
                @"Saves\Author\Scene.json",
            }));

            Assert.That(files.Single(f => f.LocalPath == @"Custom\Scripts\Author\Package\Plugin.cslist").Children.Select(f => f.LocalPath).OrderBy(f => f), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Package\src\Child1.cs",
                @"Custom\Scripts\Author\Package\src\Child2.cs",
            }));
        }
    }
}
