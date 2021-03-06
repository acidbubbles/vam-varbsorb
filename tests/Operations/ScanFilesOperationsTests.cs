﻿using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Varbsorb.Operations
{
    public class ScanFilesOperationsTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{_vamPath}\Saves\Author\Scene.json", new MockFileData(""));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Script.cs", new MockFileData(""));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Package\Plugin.cslist", new MockFileData("src/Child1.cs\r\nCustom\\Scripts\\Author\\Package\\src\\Child2.cs\r\n"));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Package\src\Child1.cs", new MockFileData("child script 1"));
            _fs.AddFile(@$"{_vamPath}\Custom\Scripts\Author\Package\src\Child2.cs", new MockFileData("child script 2"));
            _fs.AddFile(@$"{_vamPath}\Saves\scene\Custom\Morph.vmi", new MockFileData("{}"));
            _fs.AddFile(@$"{_vamPath}\Saves\scene\Custom\Morph.vmb", new MockFileData("binary"));
            var op = new ScanFilesOperation(_consoleOutput.Object, _fs);

            var files = await op.ExecuteAsync(_vamPath);

            Assert.That(files.Select(f => f.Path.Windows()).OrderBy(f => f).ToArray(), Is.EqualTo(new[]{
                @$"{_vamPath}\Custom\Scripts\Author\Package\Plugin.cslist",
                @$"{_vamPath}\Custom\Scripts\Author\Script.cs",
                @$"{_vamPath}\Saves\Author\Scene.json",
                @$"{_vamPath}\Saves\scene\Custom\Morph.vmi",
            }));

            Assert.That(files.Select(f => f.LocalPath).OrderBy(f => f).ToArray(), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Package\Plugin.cslist",
                @"Custom\Scripts\Author\Script.cs",
                @"Saves\Author\Scene.json",
                @"Saves\scene\Custom\Morph.vmi",
            }));

            Assert.That(files.Single(f => f.LocalPath == @"Custom\Scripts\Author\Package\Plugin.cslist").Children.Select(f => f.LocalPath).OrderBy(f => f).ToArray(), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Package\src\Child1.cs",
                @"Custom\Scripts\Author\Package\src\Child2.cs",
            }));

            Assert.That(files.Single(f => f.LocalPath == @"Saves\scene\Custom\Morph.vmi").Children.Select(f => f.LocalPath).OrderBy(f => f).ToArray(), Is.EqualTo(new[]{
                @"Saves\scene\Custom\Morph.vmb"
            }));
        }
    }
}
