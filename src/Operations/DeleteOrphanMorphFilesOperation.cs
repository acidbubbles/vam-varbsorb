using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteOrphanMorphFilesOperation : OperationBase, IDeleteOrphanMorphFilesOperation
    {
        protected override string Name => "Delete matched files";

        private readonly IFileSystem _fs;

        public DeleteOrphanMorphFilesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public Task ExecuteAsync(IList<FreeFile> files, IFilter filter, bool verbose, bool noop)
        {
            var filesToDelete = files
                .Where(f => !filter.IsFiltered(f.LocalPath)).SelectMany(f => f.SelfAndChildren())
                .Where(f => f.Extension == ".vmi" || f.Extension == ".vmb")
                .Select(f => (basePath: f.LocalPath.Substring(0, f.LocalPath.Length - f.Extension.Length), file: f))
                .GroupBy(x => x.basePath)
                .Where(g => g.Count() == 1)
                .Select(g => g.Single().file)
                .Where(f => f.Extension == ".vmi")
                .ToList();

            if (filesToDelete.Count == 0)
            {
                Output.WriteLine("No orphan morph files");
                return Task.CompletedTask;
            }

            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var processed = 0;
                foreach (var file in filesToDelete)
                {
                    if (verbose) Output.WriteLine($"{(noop ? "[NOOP]" : "DELETE")}: {file.LocalPath}");
                    if (!noop) _fs.File.Delete(file.Path);
                    files.Remove(file);
                    reporter.Report(new ProgressInfo(++processed, filesToDelete.Count, file.LocalPath));
                }

                if (!noop)
                {
                    foreach (var folder in filesToDelete.Select(f => _fs.Path.GetDirectoryName(f.Path)).Distinct().OrderByDescending(f => f.Length))
                    {
                        if (_fs.Directory.Exists(folder) && _fs.Directory.GetFileSystemEntries(folder).Length == 0)
                        {
                            if (verbose) Output.WriteLine($"DELETE (empty folder): {folder}");
                            _fs.Directory.Delete(folder);
                        }
                    }
                }
            }

            Output.WriteLine($"Deleted {filesToDelete.Count} orphan morph files.");

            return Task.CompletedTask;
        }
    }

    public interface IDeleteOrphanMorphFilesOperation : IOperation
    {
        Task ExecuteAsync(IList<FreeFile> files, IFilter filter, bool verbose, bool noop);
    }
}
