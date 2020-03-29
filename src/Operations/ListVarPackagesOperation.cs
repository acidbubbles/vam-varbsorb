using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Varbsorb.Operations
{
    public interface IListVarPackagesOperation : IOperation
    {
        Task<IList<VarPackage>> ExecuteAsync(string vam);
    }

    public class ListVarPackagesOperation : OperationBase, IListVarPackagesOperation
    {
        private readonly IFileSystem _fs;

        public ListVarPackagesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public Task<IList<VarPackage>> ExecuteAsync(string vam)
        {
            var packages = new List<VarPackage>();
            using (var reporter = new ProgressReporter<ListVarPackagesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                foreach (var file in _fs.Directory.GetFiles(_fs.Path.Combine(vam, "AddonPackages"), "*.var"))
                {
                    var package = new VarPackage { Path = file };
                    using var stream = _fs.File.OpenRead(file);
                    using var archive = new ZipArchive(stream);
                    foreach (var entry in archive.Entries)
                    {
                        var packageFile = new VarPackageFile { LocalPath = entry.FullName };
                        package.Files.Add(packageFile);
                    }
                    if(package.Files.Count > 0)
                        packages.Add(package);
                }
            }

            _output.WriteLine($"Found {packages.Count} packages in the AddonPackages folder.");

            return Task.FromResult((IList<VarPackage>)packages);
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