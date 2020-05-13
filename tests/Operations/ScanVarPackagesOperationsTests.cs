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
        public async Task CanIgnorePackages()
        {
            _fs.AddFile(@$"{_vamPath}\AddonPackages\Author.Package.1.var", new MockFileData(CreateFakeZip(false)));
            _fs.AddFile(@$"{_vamPath}\AddonPackages\Ignored.Package.1.var", new MockFileData(CreateFakeZip(false)));
            var op = new ScanVarPackagesOperation(_consoleOutput.Object, _fs, new SHA1HashingAlgo(), _logger);

            var files = await op.ExecuteAsync(_vamPath, GivenExcludes("Ignored.*.*.var"), VerbosityOptions.Default);

            AssertLogs();
            Assert.That(files.Select(f => f.Path).ToArray(), Is.EqualTo(new[]{
                @$"{_vamPath}\AddonPackages\Author.Package.1.var",
            }));
            Assert.That(files[0].Files.Select(f => $"{f.LocalPath}:{f.Hash}").ToList(), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Script.cslist:caaf6584e2785eaab5abe6403e87ad159e99304d",
                @"Custom\Scripts\Author\Script.cs:1ea5047cde4885663920f9941da15142a3e74919",
            }));
        }

        [Test]
        public async Task CanExecuteWith()
        {
            _fs.AddFile(@$"{_vamPath}\AddonPackages\Author.Package.1.var", new MockFileData(CreateFakeZip(true)));
            var op = new ScanVarPackagesOperation(_consoleOutput.Object, _fs, new SHA1HashingAlgo(), _logger);

            var files = await op.ExecuteAsync(_vamPath, GivenExcludes(), VerbosityOptions.Default);

            AssertLogs();
            Assert.That(files.Select(f => f.Path).ToArray(), Is.EqualTo(new[]{
                @$"{_vamPath}\AddonPackages\Author.Package.1.var",
            }));
            Assert.That(files[0].Files.Select(f => $"{f.LocalPath}:{f.Hash}").ToList(), Is.EqualTo(new[]{
                @"Custom\Scripts\Author\Script.cslist:caaf6584e2785eaab5abe6403e87ad159e99304d",
                @"Custom\Scripts\Author\Script.cs:1ea5047cde4885663920f9941da15142a3e74919",
                @"Custom\Atom\Person\Morphs\female\MyMorph.vmi:b82ce61788640c5f5f42747e58c0a1a4cf4e1880",
                @"Custom\Atom\Person\Morphs\female\MyMorph.vmb:a630e04feb2ad34ae9e63183438a99c615267d0c",
            }));
        }

        private static byte[] CreateFakeZip(bool preloadMorphs)
        {
            using var ms = new MemoryStream();
            using var archive = new ZipArchive(ms, ZipArchiveMode.Create, true);
            CreateFakeZipEntry(archive, @"Custom\Scripts\Author\Script.cslist", "Script.cs");
            CreateFakeZipEntry(archive, @"Custom\Scripts\Author\Script.cs", "public class MyScript : MVRScript { }");
            CreateFakeZipEntry(archive, @"Custom\Atom\Person\Morphs\female\MyMorph.vmi", "{ \"id\": \"My Morph\" }");
            CreateFakeZipEntry(archive, @"Custom\Atom\Person\Morphs\female\MyMorph.vmb", "[morph binary data]");
            CreateFakeZipEntry(archive, @"meta.json", "{ \"customOptions\": { \"preloadMorphs\": \"" + preloadMorphs.ToString().ToLowerInvariant() + "\" } }");
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
