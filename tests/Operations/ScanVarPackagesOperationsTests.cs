using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Varbsorb.Hashing;

namespace Varbsorb.Operations
{
    public class ScanVarPackagesOperationsTests : OperationTestsBase
    {
        [Test]
        public async Task CanExecute()
        {
            _fs.AddFile(@$"{_vamPath}\AddonPackages\Author.Package.1.var", new MockFileData(CreateFakeZip()));
            _fs.AddFile(@$"{_vamPath}\AddonPackages\Ignored.Package.1.var", new MockFileData(CreateFakeZip()));
            var op = new ScanVarPackagesOperation(_consoleOutput.Object, _fs, new SHA1HashingAlgo());

            var files = await op.ExecuteAsync(_vamPath, GivenExcludes("Ignored.*.*.var"));

            Assert.That(files.Select(f => f.Path), Is.EqualTo(new[]{
                @$"{_vamPath}\AddonPackages\Author.Package.1.var",
            }));
            Assert.That(files[0].Files.Select(f => $"{f.LocalPath}:{f.Hash}"), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Script.cslist:caaf6584e2785eaab5abe6403e87ad159e99304d",
                @"Custom\Scripts\Author\Script.cs:1ea5047cde4885663920f9941da15142a3e74919",
            }));
        }

        private static byte[] CreateFakeZip()
        {
            using var ms = new MemoryStream();
            using var archive = new ZipArchive(ms, ZipArchiveMode.Create, true);
            CreateFakeZipEntry(archive, @"Custom\Scripts\Author\Script.cslist", "Script.cs");
            CreateFakeZipEntry(archive, @"Custom\Scripts\Author\Script.cs", "public class MyScript : MVRScript { }");
            CreateFakeZipEntry(archive, @"meta.json", "{}");
            archive.Dispose();
            ms.Seek(0, SeekOrigin.Begin);
            return ms.ToArray();
        }

        private static void CreateFakeZipEntry(ZipArchive archive, string path, string contents)
        {
            var scriptEntry = archive.CreateEntry(path);
            using var writer = new StreamWriter(scriptEntry.Open());
            writer.WriteLine(contents);
        }
    }
}
