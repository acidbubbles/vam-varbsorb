using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Newtonsoft.Json;
using Varbsorb.Hashing;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class ScanVarPackagesOperation : OperationBase, IScanVarPackagesOperation
    {
        protected override string Name => "Scan var packages";

        private readonly IFileSystem _fs;
        private readonly IHashingAlgo _hashingAlgo;
        private readonly ILogger _logger;
        private readonly ConcurrentBag<VarPackage> _packages = new ConcurrentBag<VarPackage>();
        private readonly ConcurrentBag<string> _errors = new ConcurrentBag<string>();
        private readonly JsonSerializer _serializer = new JsonSerializer();
        private readonly string[] _morphExtensions = new[] { ".vmi", ".vmb" };
        private int _scanned = 0;
        private int _files = 0;

        public ScanVarPackagesOperation(IConsoleOutput output, IFileSystem fs, IHashingAlgo hashingAlgo, ILogger logger)
            : base(output)
        {
            _fs = fs;
            _hashingAlgo = hashingAlgo;
            _logger = logger;
        }

        public async Task<IList<VarPackage>> ExecuteAsync(string vam, IFilter filter, VerbosityOptions verbosity)
        {
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var packageFiles = _fs.Directory.GetFiles(_fs.Path.Combine(vam, "AddonPackages"), "*.var", SearchOption.AllDirectories);

                var scanPackageBlock = new ActionBlock<string>(
                    f => ExecuteOneAsync(reporter, filter, packageFiles.Length, f),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = 4
                    });

                foreach (var packageFile in packageFiles)
                {
                    scanPackageBlock.Post(packageFile);
                }

                scanPackageBlock.Complete();
                await scanPackageBlock.Completion;
            }

            Output.WriteLine($"Scanned {_files} files in {_packages.Count} var packages.");

            if (_errors.Count > 0)
            {
                if (verbosity == VerbosityOptions.Verbose)
                {
                    foreach (var error in _errors.OrderBy(e => e))
                    {
                        Output.WriteLine(error);
                    }
                }
                else
                {
                    Output.WriteLine($"Warning: {_errors} var packages could not be read. Run with --log or --verbose to see the details.");
                }
            }

            return _packages.ToList();
        }

        private async Task ExecuteOneAsync(IProgress<ProgressInfo> reporter, IFilter filter, int packageFilesCount, string file)
        {
            var filename = _fs.Path.GetFileName(file);
            if (!VarPackageName.TryGet(filename, out var name) || name == null) throw new VarbsorberException($"Invalid var package name: '{filename}'");
            if (filter.IsFiltered(name)) return;
            reporter.Report(new ProgressInfo(Interlocked.Increment(ref _scanned), packageFilesCount, filename));

            try
            {
                var files = new List<VarPackageFile>();
                using var stream = _fs.File.OpenRead(file);
                using var archive = new ZipArchive(stream);
                var metaEntry = archive.Entries.FirstOrDefault(e => e.FullName == "meta.json");
                if (metaEntry == null) throw new InvalidOperationException($"No meta.json available in .var package");
                dynamic? meta;
                using (var metaStream = metaEntry.Open())
                using (var streamReader = new StreamReader(metaStream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    meta = _serializer.Deserialize(jsonReader);
                }
                if (meta == null) throw new InvalidOperationException($"Could not deserialize meta.json from .var package (deserialized as null)");
                var preloadMorphs = meta.customOptions?.preloadMorphs == "true";
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(@"/")) continue;
                    if (entry.FullName == "meta.json") continue;
                    if (!preloadMorphs && _morphExtensions.Contains(_fs.Path.GetExtension(entry.FullName).ToLowerInvariant())) continue;
                    var packageFile = await ReadPackageFileAsync(entry);
                    files.Add(packageFile);
                    Interlocked.Increment(ref _files);
                }
                if (files.Count > 0)
                    _packages.Add(new VarPackage(name, file, files));
            }
            catch (Exception exc)
            {
                var message = $"[ERROR] Error loading var {filename}: {exc.Message}";
                _errors.Add(message);
                _logger.Log(message);
            }
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

    public interface IScanVarPackagesOperation : IOperation
    {
        Task<IList<VarPackage>> ExecuteAsync(string vam, IFilter filter, VerbosityOptions verbosity);
    }
}
