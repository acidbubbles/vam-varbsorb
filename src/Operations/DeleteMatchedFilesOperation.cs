using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteMatchedFilesOperation : DeleteOperationBase, IDeleteMatchedFilesOperation
    {
        protected override string Name => "Delete matched files";

        public DeleteMatchedFilesOperation(IConsoleOutput output, IFileSystem fs, IRecycleBin recycleBin)
            : base(output, fs, recycleBin)
        {
        }

        public async Task ExecuteAsync(IList<FreeFile> files, IList<FreeFilePackageMatch> matches, bool permanent, IFilter filter, bool verbose, bool noop)
        {
            var filesToDelete = new HashSet<FreeFile>();

            foreach (var match in matches)
            {
                foreach (var file in match.FreeFiles.Where(f => !filter.IsFiltered(f.LocalPath)).SelectMany(f => f.SelfAndChildren()))
                {
                    filesToDelete.Add(file);
                }
            }

            await DeleteAsync(files, filesToDelete, permanent, verbose, noop);
        }
    }

    public interface IDeleteMatchedFilesOperation : IOperation
    {
        Task ExecuteAsync(IList<FreeFile> files, IList<FreeFilePackageMatch> matches, bool permanent, IFilter filter, bool verbose, bool noop);
    }
}
