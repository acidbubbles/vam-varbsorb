using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public abstract class DeleteOperationBase : OperationBase
    {
        protected IFileSystem FileSystem { get; }
        private readonly IRecycleBin _recycleBin;
        private readonly ILogger _logger;
        private readonly object _sync = new object();
        private int _processed = 0;
        private int _errors = 0;

        public DeleteOperationBase(IConsoleOutput output, IFileSystem fs, IRecycleBin recycleBin, ILogger logger)
            : base(output)
        {
            FileSystem = fs;
            _recycleBin = recycleBin;
            _logger = logger;
        }

        public Task DeleteAsync(IList<FreeFile> files, ISet<FreeFile> filesToDelete, DeleteOptions delete, VerbosityOptions verbosity, ExecutionOptions execution)
        {
            if (filesToDelete.Count >= files.Sum(f => f.Children == null ? 1 : 1 + f.Children.Count))
                throw new InvalidOperationException($"{Name}: Attempt to delete {filesToDelete.Count}, which is more than the total scanned files.");

            if (filesToDelete.Count == 0)
            {
                Output.WriteLine($"{Name}: Nothing to delete.");
                return Task.CompletedTask;
            }

            var mbSaved = 0f;

            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                foreach (var file in filesToDelete)
                {
                    reporter.Report(new ProgressInfo(Interlocked.Increment(ref _processed), filesToDelete.Count, file.LocalPath));

                    if (verbosity == VerbosityOptions.Verbose) Output.WriteLine($"{(execution == ExecutionOptions.Noop ? "[NOOP]" : "DELETE")}: {file.LocalPath}");
                    if (execution == ExecutionOptions.Noop)
                    {
                        _logger.Log($"[DELETE(NOOP)] {file.Path}");
                        mbSaved += (file.Size ?? 0f) / 1024f / 1024f;
                    }
                    else
                    {
                        if (DeleteFile(file.Path, delete))
                        {
                            lock (_sync)
                            {
                                mbSaved += (file.Size ?? 0f) / 1024f / 1024f;
                            }
                        }
                        else
                        {
                            Interlocked.Increment(ref _errors);
                        }
                    }
                    lock (_sync)
                    {
                        files.Remove(file);
                    }
                }

                if (execution != ExecutionOptions.Noop)
                {
                    foreach (var folder in filesToDelete.Select(f => FileSystem.Path.GetDirectoryName(f.Path)).Distinct().OrderByDescending(f => f.Length))
                    {
                        if (FileSystem.Directory.Exists(folder) && FileSystem.Directory.GetFileSystemEntries(folder).Length == 0)
                        {
                            if (verbosity == VerbosityOptions.Verbose) Output.WriteLine($"DELETE (empty folder): {folder}");
                            FileSystem.Directory.Delete(folder);
                        }
                    }
                }
            }

            if (execution == ExecutionOptions.Noop)
                Output.WriteLine($"{Name}: Did not delete {filesToDelete.Count} files since --noop was specified. Estimated {mbSaved:0.00}MB would have been saved.");
            else
                Output.WriteLine($"{Name}: Deleted {filesToDelete.Count} files. Estimated {mbSaved:0.00}MB saved.");

            if (_errors > 0)
                Output.WriteLine($"{Name}: Could not delete {_errors} files. Enable logging to get more information.");

            return Task.CompletedTask;
        }

        protected bool DeleteFile(string path, DeleteOptions delete)
        {
            try
            {
                if (delete == DeleteOptions.Permanent)
                {
                    _logger.Log($"[DELETE] {path}");
                    FileSystem.File.Delete(path);
                }
                else
                {
                    _logger.Log($"[RECYCLE] {path}");
                    _recycleBin.Send(path);
                }

                return true;
            }
            catch (Exception exc)
            {
                _logger.Log($"Failed to delete '{path}': {exc.Message}");

                return false;
            }
        }
    }
}
