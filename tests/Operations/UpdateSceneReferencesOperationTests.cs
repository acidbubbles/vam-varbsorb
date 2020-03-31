using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class UpdateSceneReferencesOperationTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\scene\MyScene.json", new MockFileData(@"{""id"":""Custom\Scripts\Script1.cs"", ""path"":""Script1.cs""}"));
            var op = new UpdateSceneReferencesOperation(_consoleOutput.Object, _fs);
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
                new FreeFilePackageMatch(
                     new VarPackage(new VarPackageName("Author.Name.1.var"), "absolute-path", new List<VarPackageFile>()),
                     new VarPackageFile(@"Custom\Scripts\MyScript.cs", "hash"),
                     new[] { scriptFile }
                )
            };

            var files = await op.ExecuteAsync(scenes, matches);

            Assert.That(_fs.GetFile($@"{_vamPath}\Saves\scene\MyScene.json").TextContents, Is.EqualTo(@"{""id"":""Author.Name.1:/Custom/Scripts/MyScript.cs"", ""path"":""Author.Name.1:/Custom/Scripts/MyScript.cs""}"));
        }
    }
}
