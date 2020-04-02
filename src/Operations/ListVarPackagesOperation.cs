using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using Varbsorb.Hashing;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class ListVarPackagesOperation : OperationBase, IListVarPackagesOperation
    {
        protected override string Name => "Scan var packages";

        private readonly IFileSystem _fs;
        private readonly IHashingAlgo _hashingAlgo;

        public ListVarPackagesOperation(IConsoleOutput output, IFileSystem fs, IHashingAlgo hashingAlgo)
            : base(output)
        {
            _fs = fs;
            _hashingAlgo = hashingAlgo;
        }

        public async Task<IList<VarPackage>> ExecuteAsync(string vam)
        {
            var packages = new List<VarPackage>();
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var packagesScanned = 0;
                var packageFiles = _fs.Directory.GetFiles(_fs.Path.Combine(vam, "AddonPackages"), "*.var");
                foreach (var file in packageFiles)
                {
                    var filename = _fs.Path.GetFileName(file);
                    var files = new List<VarPackageFile>();
                    using var stream = _fs.File.OpenRead(file);
                    using var archive = new ZipArchive(stream);
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(@"/")) continue;
                        if (entry.FullName == "meta.json") continue;
                        var packageFile = await ReadPackageFileAsync(entry);
                        files.Add(packageFile);
                    }
                    if (files.Count > 0)
                        packages.Add(new VarPackage(new VarPackageName(filename), file, files));

                    reporter.Report(new ProgressInfo(++packagesScanned, packageFiles.Length, filename));
                }
            }

            Output.WriteLine($"Scanned {packages.Count} packages.");

            return packages;
        }

        private async Task<VarPackageFile> ReadPackageFileAsync(ZipArchiveEntry entry)
        {
            using var entryMemoryStream = new MemoryStream();
            using (var entryStream = entry.Open())
            {
                await entryStream.CopyToAsync(entryMemoryStream);
            }
            var hash = _hashingAlgo.GetHash(entryMemoryStream.ToArray());
            return new VarPackageFile(entry.FullName.NormalizePathSeparators(), hash);
        }
    }

    public interface IListVarPackagesOperation : IOperation
    {
        Task<IList<VarPackage>> ExecuteAsync(string vam);
    }
}
