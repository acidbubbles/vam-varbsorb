using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class UpdateSceneReferencesOperationTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\scene\MyScene.json", new MockFileData(@"{""id"":""Custom\Saves\scene\MyScene.json""}"));
            var op = new UpdateSceneReferencesOperation(_consoleOutput.Object, _fs);
            var scriptFile = new FreeFile("", @"Custom\Scripts\MyScript.cs", "myscript.cs", ".cs");
            var scenes = GivenFiles(@"Saves\scene\MyScene.json").Select(f => new SceneFile(f)
            {
                References = new List<SceneReference>
                {
                    new SceneReference{File = scriptFile, Index = 7, Length = 31}
                }
            }).ToList();
            var matches = new List<FreeFilePackageMatch>
            {
                new FreeFilePackageMatch
                {
                    Package = new VarPackage
                    {
                        Name = new VarPackageName("Author.Name.1.var")
                    },
                    PackageFile = new VarPackageFile(@"Custom\Scripts\MyScript.cs", "myscript.cs", ".cs"),
                    FreeFiles = new[] { scriptFile },
                }
            };

            var files = await op.ExecuteAsync(scenes, matches);

            Assert.That(_fs.GetFile($@"{_vamPath}\Saves\scene\MyScene.json").TextContents, Is.EqualTo(@"{""id"":""Author.Name.1:/Custom/Scripts/MyScript.cs""}"));
        }
    }
}
