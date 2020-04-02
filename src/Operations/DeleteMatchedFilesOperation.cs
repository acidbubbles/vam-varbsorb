using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteMatchedFilesOperation : OperationBase, IDeleteMatchedFilesOperation
    {
        public const int MaxVerbose = 100;

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
                if (noop) Output.WriteLine("* Overriden by --noop, no files will actually be deleted.");
                foreach (var file in files.Take(MaxVerbose))
                {
                    if (verbose) Output.WriteLine($"- {file.LocalPath}");
                    if (!noop) _fs.File.Delete(file.Path);
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
