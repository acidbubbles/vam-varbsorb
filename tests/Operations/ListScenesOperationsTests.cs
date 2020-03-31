using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace Varbsorb.Operations
{
    public class ListScenesOperationsTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\scene\party\Party.json", new MockFileData(@"{
                ""uid"" : ""ScriptRel.cs"",
                ""id"": ""Custom\Scripts\ScriptAbs.cs"",
                ""path"": ""Custom\Scripts\Missing.cs"",
                ""prop"": ""Saves\Scripts\Legacy.cs"",
            }"));
            _fs.AddFile(@$"{_vamPath}\Saves\scene\party\ScriptRel.cs", new MockFileData("public class ScriptRel : MVRScript {}"));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\ScriptAbs.cs", new MockFileData("public class ScriptAbs : MVRScript {}"));
            var op = new ListScenesOperation(_consoleOutput.Object, _fs);
            var files = GivenFiles(
                @"Saves\scene\party\Party.json",
                @"Saves\scene\party\ScriptRel.cs",
                @"Custom\Scripts\ScriptAbs.cs",
                @"Custom\Scripts\Legacy.cs"
            );

            var scenes = await op.ExecuteAsync(_vamPath, files);

            Assert.That(scenes.Count, Is.EqualTo(1));

            Assert.That(scenes[0].References.Select(f => $"{f.File.LocalPath}[{f.Index}-{f.Length}]").OrderBy(f => f), Is.EqualTo(new[]{
                @"Custom\Scripts\Legacy.cs[173-23]",
                @"Custom\Scripts\ScriptAbs.cs[65-27]",
                @"Saves\scene\party\ScriptRel.cs[27-12]",
            }));
        }
    }
}
