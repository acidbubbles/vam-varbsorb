using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class DeleteOrphanMorphFilesOperation : DeleteOperationBase, IDeleteOrphanMorphFilesOperation
    {
        protected override string Name => "Delete matched files";

        public DeleteOrphanMorphFilesOperation(IConsoleOutput output, IFileSystem fs, IRecycleBin recycleBin)
            : base(output, fs, recycleBin)
        {
        }

        public async Task ExecuteAsync(IList<FreeFile> files, bool permanent, IFilter filter, bool verbose, bool noop)
        {
            var filesToDelete = new HashSet<FreeFile>(files
                .Where(f => !filter.IsFiltered(f.LocalPath)).SelectMany(f => f.SelfAndChildren())
                .Where(f => f.Extension == ".vmi" || f.Extension == ".vmb")
                .Select(f => (basePath: f.LocalPath.Substring(0, f.LocalPath.Length - f.Extension.Length), file: f))
                .GroupBy(x => x.basePath)
                .Where(g => g.Count() == 1)
                .Select(g => g.Single().file)
                .Where(f => f.Extension == ".vmi"));

            await DeleteAsync(files, filesToDelete, permanent, verbose, noop);
        }
    }

    public interface IDeleteOrphanMorphFilesOperation : IOperation
    {
        Task ExecuteAsync(IList<FreeFile> files, bool permanent, IFilter filter, bool verbose, bool noop);
    }
}
