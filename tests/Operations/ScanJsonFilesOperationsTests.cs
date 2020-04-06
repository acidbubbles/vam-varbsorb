using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Varbsorb.Operations
{
    public class ScanJsonFilesOperationsTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\scene\party\Party.json", new MockFileData(@"{
                ""uid"" : ""ScriptRel.cs"",
                ""url"": ""Custom\Scripts\ScriptAbs.cs"",
                ""plugin#3"": ""Custom\Scripts\Missing.cs"",
                ""assetUrl"": ""Saves\Scripts\Legacy.cs"",
            }".Replace("\r\n", "\n")));
            _fs.AddFile(@$"{_vamPath}\Saves\scene\ignored\anything.json", new MockFileData(@"{
                ""uid"" : ""filtered.cs"",
            }".Replace("\r\n", "\n")));
            _fs.AddFile(@$"{_vamPath}\Saves\scene\party\ScriptRel.cs", new MockFileData("public class ScriptRel : MVRScript {}"));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\ScriptAbs.cs", new MockFileData("public class ScriptAbs : MVRScript {}"));
            var op = new ScanJsonFilesOperation(_consoleOutput.Object, _fs);
            var files = GivenFiles(
                @"Saves\scene\party\Party.json",
                @"Saves\scene\party\ScriptRel.cs",
                @"Custom\Scripts\ScriptAbs.cs",
                @"Custom\Scripts\Legacy.cs"
            );

            var scenes = await op.ExecuteAsync(_vamPath, files, Filter.From(null, new[] { @"Saves\scene\ignored\" }), ErrorReportingOptions.None);

            Assert.That(scenes.Count, Is.EqualTo(1));

            Assert.That(scenes[0].References.Select(f => $"{f.File.LocalPath}[{f.Index}-{f.Length}]").OrderBy(f => f), Is.EqualTo(new[]
            {
                @"Custom\Scripts\Legacy.cs[182-23]",
                @"Custom\Scripts\ScriptAbs.cs[66-27]",
                @"Saves\scene\party\ScriptRel.cs[27-12]",
            }));

            Assert.That(scenes[0].Missing.OrderBy(f => f), Is.EqualTo(new[]
            {
                @"Custom\Scripts\Missing.cs",
            }));
        }
    }
}
