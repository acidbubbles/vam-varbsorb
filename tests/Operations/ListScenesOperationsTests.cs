using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class ListScenesOperationsTests
    {
        private const string _vamPath = @"C:\Vam";

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
            _fs.AddFile(@$"{_vamPath}\Saves\scene\party\Party.json", new MockFileData(@"{
                ""uid"" : ""ScriptRel.cs"",
                ""id"": ""Custom\Scripts\ScriptAbs.cs"",
                ""path"": ""Custom\Scripts\Missing.cs"",
            }"));
            _fs.AddFile(@$"{_vamPath}\Saves\scene\party\ScriptRel.cs", new MockFileData("public class ScriptRel : MVRScript {}"));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\ScriptAbs.cs", new MockFileData("public class ScriptAbs : MVRScript {}"));
            var op = new ListScenesOperation(_consoleOutput.Object, _fs);
            var files = GivenFiles(
                @"Saves\scene\party\Party.json",
                @"Saves\scene\party\ScriptRel.cs",
                @"Custom\Scripts\ScriptAbs.cs"
            );

            var scenes = await op.ExecuteAsync(_vamPath, files);

            Assert.That(scenes.Count, Is.EqualTo(1));

            Assert.That(scenes[0].References.Select(f => $"{f.File.LocalPath}[{f.Index}-{f.Length}]").OrderBy(f => f), Is.EqualTo(new[]{
                @"Custom\Scripts\ScriptAbs.cs[65-27]",
                @"Saves\scene\party\ScriptRel.cs[27-12]",
            }));
        }

        private IList<FreeFile> GivenFiles(params string[] files)
        {
            return files.Select(f => new FreeFile(
                 $@"{_vamPath}\{f}",
                 f,
                 Path.GetFileName(f).ToLowerInvariant(),
                 Path.GetExtension(f)
            )).ToList();
        }
    }
}
