using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteMatchedFilesOperation : OperationBase, IDeleteMatchedFilesOperation
    {
        protected override string Name => "Delete matched files";

        private readonly IFileSystem _fs;

        public DeleteMatchedFilesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public Task ExecuteAsync(IList<FreeFilePackageMatch> matches, IFilter filter, bool verbose, bool noop)
        {
            var files = new HashSet<FreeFile>();

            foreach (var match in matches)
            {
                foreach (var file in match.FreeFiles.Where(f => !filter.IsFiltered(f.LocalPath)).SelectMany(f => f.SelfAndChildren()))
                {
                    files.Add(file);
                }
            }

            if (files.Count > 0)
            {
                var mbSaved = files.Sum(f => f.Size) / 1024f / 1024f;
                Output.WriteLine($"{files.Count} files will be deleted. Estimated {mbSaved:0.00}MB saved.");
                foreach (var file in files)
                {
                    if (verbose) Output.WriteLine($"{(noop ? "[NOOP]" : "DELETE")}: {file.LocalPath}");
                    if (!noop) _fs.File.Delete(file.Path);
                }

                if (!noop)
                {
                    foreach (var folder in files.Select(f => _fs.Path.GetDirectoryName(f.Path)).Distinct().OrderByDescending(f => f.Length))
                    {
                        if (_fs.Directory.Exists(folder) && _fs.Directory.GetFileSystemEntries(folder).Length == 0)
                        {
                            if (verbose) Output.WriteLine($"DELETE (empty folder): {folder}");
                            _fs.Directory.Delete(folder);
                        }
                    }
                }
            }
            else
            {
                Output.WriteLine("Good news, there's nothing to delete!");
            }

            return Task.CompletedTask;
        }
    }

    public interface IDeleteMatchedFilesOperation : IOperation
    {
        Task ExecuteAsync(IList<FreeFilePackageMatch> matches, IFilter filter, bool verbose, bool noop);
    }
}
