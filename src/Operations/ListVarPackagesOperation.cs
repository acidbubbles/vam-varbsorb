using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public interface IListVarPackagesOperation : IOperation
    {
        Task<IList<VarPackage>> ExecuteAsync(string vam);
    }

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
                foreach (var file in _fs.Directory.GetFiles(_fs.Path.Combine(vam, "AddonPackages"), "*.var"))
                {
                    var package = new VarPackage
                    {
                        Name = new VarPackageName(_fs.Path.GetFileName(file)),
                        Path = file
                    };
                    using var stream = _fs.File.OpenRead(file);
                    using var archive = new ZipArchive(stream);
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(@"/")) continue;
                        if (entry.FullName == "meta.json") continue;
                        var packageFile = await ReadPackageFileAsync(entry);
                        package.Files.Add(packageFile);
                    }
                    if (package.Files.Count > 0)
                        packages.Add(package);
                }
            }

            _output.WriteLine($"Found {packages.Count} packages in the AddonPackages folder.");

            return packages;
        }

        private async Task<VarPackageFile> ReadPackageFileAsync(ZipArchiveEntry entry)
        {
            var packageFile = new VarPackageFile(
                entry.FullName.Replace('/', '\\'),
                _fs.Path.GetFileName(entry.FullName.ToLowerInvariant()),
                _fs.Path.GetExtension(entry.FullName).ToLowerInvariant()
            );
            using var entryMemoryStream = new MemoryStream();
            using (var entryStream = entry.Open())
            {
                await entryStream.CopyToAsync(entryMemoryStream);
            }
            packageFile.Hash = _hashingAlgo.GetHash(entryMemoryStream.ToArray());
            return packageFile;
        }

        public class ListVarPackagesProgress
        {
            public int Packages { get; set; }
            public int Files { get; set; }
            public string CurrentPackage { get; set; }
        }

        private void ReportProgress(ListVarPackagesProgress progress)
        {
            _output.WriteAndReset($"Scanning... {progress.Files} files discovered in {progress.Packages} packages: {progress.CurrentPackage}");
        }
    }
}