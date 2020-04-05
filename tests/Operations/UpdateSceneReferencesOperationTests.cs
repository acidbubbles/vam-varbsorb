using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class UpdateSceneReferencesOperationTests : OperationTestsBase
    {
        [Test]
        public async Task CanReplacePaths()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\scene\MyScene.json", new MockFileData(@"{""id"":""Custom\Scripts\Script1.cs"", ""path"":""Script1.cs""}"));
            var op = new UpdateSceneReferencesOperation(_consoleOutput.Object, _fs, Mock.Of<ILogger>());
            var scriptFile = new FreeFile("", @"Custom\Scripts\MyScript.cs");
            var scenes = GivenFiles(@"Saves\scene\MyScene.json").Select(f => new SceneFile(f, new List<SceneReference>
                {
                    new SceneReference(scriptFile, 7, 25),
                    new SceneReference(scriptFile, 43, 10),
                },
                new List<string>()
            )).ToList();
            var matches = new List<FreeFilePackageMatch>
            {
                GivenPackageMatch("Author.Name.1.var", scriptFile)
            };

            await op.ExecuteAsync(scenes, matches, ExecutionOptions.Default);

            Assert.That(_fs.GetFile($@"{_vamPath}\Saves\scene\MyScene.json").TextContents, Is.EqualTo(@"{""id"":""Author.Name.1:/Custom/Scripts/MyScript.cs"", ""path"":""Author.Name.1:/Custom/Scripts/MyScript.cs""}"));
        }

        [Test]
        public async Task SelectsMostRecentAndSmall()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\scene\MyScene.json", new MockFileData(@"{""id"":""Custom\Scripts\Script1.cs""}"));
            var op = new UpdateSceneReferencesOperation(_consoleOutput.Object, _fs, Mock.Of<ILogger>());
            var scriptFile = new FreeFile("", @"Custom\Scripts\MyScript.cs");
            var scenes = GivenFiles(@"Saves\scene\MyScene.json").Select(f => new SceneFile(f, new List<SceneReference>
                {
                    new SceneReference(scriptFile, 7, 25)
                },
                new List<string>()
            )).ToList();
            var matches = new List<FreeFilePackageMatch>
            {
                GivenPackageMatch("Author.Name.1.var", scriptFile),
                GivenPackageMatch("Author.Name.2.var", scriptFile),
                GivenPackageMatch("Author.Other.3.var", scriptFile, 2),
            };

            await op.ExecuteAsync(scenes, matches, ExecutionOptions.Default);

            Assert.That(_fs.GetFile($@"{_vamPath}\Saves\scene\MyScene.json").TextContents, Is.EqualTo(@"{""id"":""Author.Name.2:/Custom/Scripts/MyScript.cs""}"));
        }

        private static FreeFilePackageMatch GivenPackageMatch(string filename, FreeFile scriptFile, int additionalFiles = 0)
        {
            return new FreeFilePackageMatch(
                new VarPackage(
                    new VarPackageName(filename),
                    "absolute-path",
                    Enumerable
                    .Range(0, 1 + additionalFiles)
                    .Select(i => new VarPackageFile($@"Custom\Scripts\{i}.cs", $"hash:{i}"))
                    .ToList()),
                new VarPackageFile(@"Custom\Scripts\MyScript.cs", "hash"),
                new[] { scriptFile });
        }
    }
}
