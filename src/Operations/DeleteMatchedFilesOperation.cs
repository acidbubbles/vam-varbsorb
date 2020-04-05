using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteMatchedFilesOperation : DeleteOperationBase, IDeleteMatchedFilesOperation
    {
        protected override string Name => "Delete duplicate files";

        public DeleteMatchedFilesOperation(IConsoleOutput output, IFileSystem fs, IRecycleBin recycleBin, ILogger logger)
            : base(output, fs, recycleBin, logger)
        {
        }

        public async Task ExecuteAsync(IList<FreeFile> files, IList<FreeFilePackageMatch> matches, DeleteOptions delete, IFilter filter, VerbosityOptions verbosity, ExecutionOptions execution)
        {
            var filesToDelete = new HashSet<FreeFile>();

            foreach (var match in matches)
            {
                foreach (var file in match.FreeFiles.Where(f => !filter.IsFiltered(f.LocalPath)).SelectMany(f => f.SelfAndChildren()))
                {
                    filesToDelete.Add(file);
                }
            }

            await DeleteAsync(files, filesToDelete, delete, verbosity, execution);
        }
    }

    public interface IDeleteMatchedFilesOperation : IOperation
    {
        Task ExecuteAsync(IList<FreeFile> files, IList<FreeFilePackageMatch> matches, DeleteOptions delete, IFilter filter, VerbosityOptions verbosity, ExecutionOptions execution);
    }
}
