using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
        private int _processed;

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
                var matchPackageBlock = new ActionBlock<VarPackage>(
                    package => MatchPackageAsync(reporter, package, packages.Count, freeFilesSet),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = 4
                    });

                foreach (var package in packages)
                {
                    matchPackageBlock.Post(package);
                }

                matchPackageBlock.Complete();
                await matchPackageBlock.Completion;
            }

            Output.WriteLine($"Matched {_matches.Count} files.");

            return _matches.ToList();
        }

        private async Task MatchPackageAsync(IProgress<ProgressInfo> reporter, VarPackage package, int packagesCount, ConcurrentDictionary<string, List<FreeFile>> freeFilesSet)
        {
            reporter.Report(new ProgressInfo(Interlocked.Increment(ref _processed), packagesCount, package.Name.Filename));

            await Task.WhenAll(package.Files.Select(async f => await MatchPackageFileAsync(package, f, freeFilesSet)));
        }

        private async Task MatchPackageFileAsync(VarPackage package, VarPackageFile packageFile, ConcurrentDictionary<string, List<FreeFile>> freeFilesSet)
        {
            if (!freeFilesSet.TryGetValue(packageFile.FilenameLower, out var matchingFreeFiles))
                return;

            await Task.WhenAll(matchingFreeFiles.SelectMany(m => m.SelfAndChildren()).Where(m => m.Hash == null).Select(async m => await ComputeHashAsync(m)));

            var matchedFreeFiles = matchingFreeFiles
                .Where(ff => ff.Hash == packageFile.Hash)
                .Where(ff =>
                {
                    if (ff.Children == null || ff.Children.Count == 0) return true;
                    foreach (var child in ff.Children)
                    {
                        if (!package.Files.Any(pf => pf.FilenameLower == child.FilenameLower && pf.Hash == child.Hash))
                            return false;
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
