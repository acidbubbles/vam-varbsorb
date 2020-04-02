using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Hashing;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class MatchFilesToPackagesOperation : OperationBase, IMatchFilesToPackagesOperation
    {
        protected override string Name => "Match packages to files";

        private readonly IFileSystem _fs;
        private readonly IHashingAlgo _hashingAlgo;

        public MatchFilesToPackagesOperation(IConsoleOutput output, IFileSystem fs, IHashingAlgo hashingAlgo)
            : base(output)
        {
            _fs = fs;
            _hashingAlgo = hashingAlgo;
        }

        public async Task<IList<FreeFilePackageMatch>> ExecuteAsync(IList<VarPackage> packages, IList<FreeFile> freeFiles)
        {
            var freeFilesSet = freeFiles.GroupBy(ff => ff.FilenameLower).ToDictionary(f => f.Key, f => f.ToList());
            var matches = new List<FreeFilePackageMatch>();
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var packagesComplete = 0;
                foreach (var package in packages)
                {
                    foreach (var packageFile in package.Files)
                    {
                        if (!freeFilesSet.TryGetValue(packageFile.FilenameLower, out var matchingFreeFiles))
                            continue;

                        foreach (var matchingFreeFile in matchingFreeFiles)
                        {
                            if (matchingFreeFile.Hash != null) continue;
                            var bytes = await _fs.File.ReadAllBytesAsync(matchingFreeFile.Path);
                            matchingFreeFile.Size = bytes.Length;
                            matchingFreeFile.Hash = _hashingAlgo.GetHash(bytes);
                        }
                        var matchedFreeFiles = matchingFreeFiles
                            .Where(ff =>
                            {
                                if (ff.Hash != packageFile.Hash) return false;
                                if (ff.Children != null)
                                {
                                    foreach (var child in ff.Children)
                                    {
                                        if (!package.Files.Any(pf => pf.FilenameLower == child.FilenameLower && pf.Hash == child.Hash))
                                            return false;
                                    }
                                }
                                return true;
                            })
                            .ToList();

                        if (matchedFreeFiles.Count == 0)
                            continue;

                        matches.Add(new FreeFilePackageMatch(
                            package,
                            packageFile,
                            matchedFreeFiles));
                    }

                    reporter.Report(new ProgressInfo(++packagesComplete, packages.Count, package.Name.Filename));
                }
            }

            Output.WriteLine($"Matched {matches.Count} files.");

            return matches;
        }
    }

    public interface IMatchFilesToPackagesOperation : IOperation
    {
        Task<IList<FreeFilePackageMatch>> ExecuteAsync(IList<VarPackage> packages, IList<FreeFile> freeFiles);
    }
}
