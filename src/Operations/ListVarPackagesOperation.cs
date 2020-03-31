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
            using (var reporter = new ProgressReporter<ListVarPackagesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                var packagesScanned = 0;
                foreach (var file in _fs.Directory.GetFiles(_fs.Path.Combine(vam, "AddonPackages"), "*.var"))
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

                    reporter.Report(new ListVarPackagesProgress(++packagesScanned, filename));
                }
            }

            Output.WriteLine($"Found {packages.Count} packages in the AddonPackages folder.");

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
            return new VarPackageFile(entry.FullName.Normalize(), hash);
        }

        public class ListVarPackagesProgress
        {
            public ListVarPackagesProgress(int packagesScanned, string filename)
            {
                PackagesScanned = packagesScanned;
                CurrentPackage = filename;
            }

            public int PackagesScanned { get; set; }
            public string CurrentPackage { get; set; }
        }

        private void ReportProgress(ListVarPackagesProgress progress)
        {
            Output.WriteAndReset($"Scanning packages... {progress.PackagesScanned} scanned: {progress.CurrentPackage}");
        }
    }

    public interface IListVarPackagesOperation : IOperation
    {
        Task<IList<VarPackage>> ExecuteAsync(string vam);
    }
}
