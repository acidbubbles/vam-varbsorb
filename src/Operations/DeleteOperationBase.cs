using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public abstract class DeleteOperationBase : OperationBase
    {
        private readonly IRecycleBin _recycleBin;
        private readonly object _sync = new object();

        protected IFileSystem FileSystem { get; }

        public DeleteOperationBase(IConsoleOutput output, IFileSystem fs, IRecycleBin recycleBin)
            : base(output)
        {
            FileSystem = fs;
            _recycleBin = recycleBin;
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

            var mbSaved = filesToDelete.Sum(f => (long)(f.Size ?? 0)) / 1024f / 1024f;
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var processed = 0;
                foreach (var file in filesToDelete)
                {
                    if (verbosity == VerbosityOptions.Verbose) Output.WriteLine($"{(execution == ExecutionOptions.Noop ? "[NOOP]" : "DELETE")}: {file.LocalPath}");
                    if (execution != ExecutionOptions.Noop) DeleteFile(file.Path, delete);
                    lock (_sync)
                    {
                        files.Remove(file);
                    }
                    reporter.Report(new ProgressInfo(++processed, filesToDelete.Count, file.LocalPath));
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

            Output.WriteLine($"{Name}: Deleted {filesToDelete.Count} files. Estimated {mbSaved:0.00}MB saved.");

            return Task.CompletedTask;
        }

        protected void DeleteFile(string path, DeleteOptions delete)
        {
            if (delete == DeleteOptions.Permanent)
            {
                FileSystem.File.Delete(path);
            }
            else
            {
                _recycleBin.Send(path);
            }
        }
    }
}
