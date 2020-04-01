using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class ListUnusedFilesOperation : OperationBase, IListUnusedFilesOperation
    {
        public ListUnusedFilesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
        }

        public Task<ISet<FreeFile>> ExecuteAsync(IList<FreeFilePackageMatch> matches)
        {
            var files = new HashSet<FreeFile>();

            foreach (var match in matches)
            {
                foreach (var file in match.FreeFiles.SelectMany(f => f.SelfAndChildren()))
                {
                    files.Add(file);
                }
            }

            return Task.FromResult((ISet<FreeFile>)files);
        }
    }

    public interface IListUnusedFilesOperation : IOperation
    {
        Task<ISet<FreeFile>> ExecuteAsync(IList<FreeFilePackageMatch> matches);
    }
}
