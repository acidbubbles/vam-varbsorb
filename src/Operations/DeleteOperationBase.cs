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

        protected IFileSystem FileSystem { get; }

        public DeleteOperationBase(IConsoleOutput output, IFileSystem fs, IRecycleBin recycleBin)
            : base(output)
        {
            FileSystem = fs;
            _recycleBin = recycleBin;
        }

        public Task DeleteAsync(IList<FreeFile> files, ISet<FreeFile> filesToDelete, bool permanent, bool verbose, bool noop)
        {
            if (filesToDelete.Count >= files.Sum(f => f.Children == null ? 1 : 1 + f.Children.Count))
                throw new InvalidOperationException($"Attempt to delete {filesToDelete.Count}, which is more than the total scanned files.");

            if (filesToDelete.Count == 0)
            {
                Output.WriteLine("Nothing to delete.");
                return Task.CompletedTask;
            }

            var mbSaved = filesToDelete.Sum(f => (long)(f.Size ?? 0)) / 1024f / 1024f;
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var processed = 0;
                foreach (var file in filesToDelete)
                {
                    if (verbose) Output.WriteLine($"{(noop ? "[NOOP]" : "DELETE")}: {file.LocalPath}");
                    if (!noop) DeleteFile(file.Path, permanent);
                    files.Remove(file);
                    reporter.Report(new ProgressInfo(++processed, filesToDelete.Count, file.LocalPath));
                }

                if (!noop)
                {
                    foreach (var folder in filesToDelete.Select(f => FileSystem.Path.GetDirectoryName(f.Path)).Distinct().OrderByDescending(f => f.Length))
                    {
                        if (FileSystem.Directory.Exists(folder) && FileSystem.Directory.GetFileSystemEntries(folder).Length == 0)
                        {
                            if (verbose) Output.WriteLine($"DELETE (empty folder): {folder}");
                            FileSystem.Directory.Delete(folder);
                        }
                    }
                }
            }

            Output.WriteLine($"Deleted {filesToDelete.Count} files. Estimated {mbSaved:0.00}MB saved.");

            return Task.CompletedTask;
        }

        protected void DeleteFile(string path, bool permanent)
        {
            if (permanent)
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
