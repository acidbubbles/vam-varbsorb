using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace Varbsorb.Operations
{
    public interface IMatchFilesToPackagesOperation : IOperation
    {
        Task<IList<FreeFilePackageMatch>> ExecuteAsync(IList<VarPackage> packages, IList<FreeFile> freeFiles);
    }

    public class MatchFilesToPackagesOperation : OperationBase, IMatchFilesToPackagesOperation
    {
        private readonly IFileSystem _fs;

        public MatchFilesToPackagesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public Task<IList<FreeFilePackageMatch>> ExecuteAsync(IList<VarPackage> packages, IList<FreeFile> freeFiles)
        {
            var freeFilesSet = freeFiles.GroupBy(ff => _fs.Path.GetFileName(ff.Path).ToLowerInvariant()).ToDictionary(f => f.Key, f => f.ToList());
            var matches = new List<FreeFilePackageMatch>();
            using (var reporter = new ProgressReporter<MatchFilesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                foreach(var package in packages)
                {
                    foreach(var packageFile in package.Files)
                    {
                        if(freeFilesSet.TryGetValue(_fs.Path.GetFileName(packageFile.LocalPath).ToLowerInvariant(), out var freeFile))
                        {
                            // TODO: Select the _best_ match (hash and complete set)
                            var match = new FreeFilePackageMatch
                            {
                                Package = package,
                                PackageFile = packageFile,
                                FreeFile = freeFile[0]
                            };
                            matches.Add(match);
                            // TODO: Progress
                        }
                    }
                }
            }

            _output.WriteLine($"Found {matches.Count} matching files.");

            return Task.FromResult((IList<FreeFilePackageMatch>)matches);
        }

        public class MatchFilesProgress
        {
            public int Files { get; set; }
            public int Total { get; set; }
        }

        private void ReportProgress(MatchFilesProgress progress)
        {
            _output.WriteAndReset($"Matching packages to files... {progress.Files} / {progress.Total} ({progress.Files / (float)progress.Total * 100:0}%)");
        }
    }
}