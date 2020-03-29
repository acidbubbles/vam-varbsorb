using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Varbsorb.Operations
{
    public class
ListFilesOperationsTests

    {
        private const string VamPath = @"C:\Vam";
        private Mock<IConsoleOutput> _consoleOutput;
        private MockFileSystem _fs;

        [SetUp]
        public void Setup()
        {
            _consoleOutput = new Mock<IConsoleOutput>(MockBehavior.Loose);
            _fs = new MockFileSystem();
        }

        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{VamPath}\Saves\Author\Scene.json", new MockFileData(""));
            _fs.AddFile(@$"{VamPath}\Custom\Scripts\Author\Script.cs", new MockFileData(""));
            var op = new ListFilesOperation(_consoleOutput.Object, _fs);

            var files = await op.ExecuteAsync(VamPath);

            Assert.That(files.Select(f => f.Path).OrderBy(f => f), Is.EqualTo(new[]{
                @$"{VamPath}\Custom\Scripts\Author\Script.cs",
                @$"{VamPath}\Saves\Author\Scene.json",
            }));

            Assert.That(files.Select(f => f.LocalPath).OrderBy(f => f), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Script.cs",
                @"Saves\Author\Scene.json",
            }));
        }
    }
}