using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
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
        private readonly ConcurrentBag<FreeFilePackageMatch> _matches = new ConcurrentBag<FreeFilePackageMatch>();
        private int _processed = 0;

        public MatchFilesToPackagesOperation(IConsoleOutput output, IFileSystem fs, IHashingAlgo hashingAlgo)
            : base(output)
        {
            _fs = fs;
            _hashingAlgo = hashingAlgo;
        }

        public async Task<IList<FreeFilePackageMatch>> ExecuteAsync(IList<VarPackage> packages, IList<FreeFile> freeFiles)
        {
            var freeFilesSet = new ConcurrentDictionary<string, List<FreeFile>>(freeFiles.GroupBy(ff => ff.FilenameLower).ToDictionary(f => f.Key, f => f.ToList()));
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                await Task.WhenAll(packages.Select(package => MatchPackageAsync(reporter, package, packages.Count, freeFilesSet)));
            }

            Output.WriteLine($"Matched {_matches.Count} files.");

            return _matches.ToList();
        }

        private async Task MatchPackageAsync(IProgress<ProgressInfo> reporter, VarPackage package, int packagesCount, ConcurrentDictionary<string, List<FreeFile>> freeFilesSet)
        {
            await Task.WhenAll(package.Files.Select(f => MatchPackageFileAsync(package, f, freeFilesSet)));

            var scanned = Interlocked.Increment(ref _processed);
            reporter.Report(new ProgressInfo(_processed, packagesCount, package.Name.Filename));
        }

        private async Task MatchPackageFileAsync(VarPackage package, VarPackageFile packageFile, ConcurrentDictionary<string, List<FreeFile>> freeFilesSet)
        {
            if (!freeFilesSet.TryGetValue(packageFile.FilenameLower, out var matchingFreeFiles))
                return;

            await Task.WhenAll(matchingFreeFiles.Where(m => m.Hash == null).Select(m => ComputeHashAsync(m)));

            var matchedFreeFiles = matchingFreeFiles
                .Where(ff => ff.Hash == packageFile.Hash)
                .Where(ff =>
                {
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
                return;

            _matches.Add(new FreeFilePackageMatch(
                package,
                packageFile,
                matchedFreeFiles));
        }

        private async Task ComputeHashAsync(FreeFile matchingFreeFile)
        {
            var bytes = await _fs.File.ReadAllBytesAsync(matchingFreeFile.Path);
            matchingFreeFile.Size = bytes.Length;
            matchingFreeFile.Hash = _hashingAlgo.GetHash(bytes);
        }
    }

    public interface IMatchFilesToPackagesOperation : IOperation
    {
        Task<IList<FreeFilePackageMatch>> ExecuteAsync(IList<VarPackage> packages, IList<FreeFile> freeFiles);
    }
}
