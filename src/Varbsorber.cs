using System.Threading.Tasks;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class Varbsorber
    {
        private readonly IConsoleOutput _output;
        private readonly IOperationsFactory _operationsFactory;

        public Varbsorber(IConsoleOutput output, IOperationsFactory operationsFactory)
        {
            _output = output;
            _operationsFactory = operationsFactory;
        }

        public async Task ExecuteAsync(string vam, bool noop)
        {
            var varFiles = await _operationsFactory.Get<IListVarPackagesOperation>().ExecuteAsync(vam);
            var freeFiles = await _operationsFactory.Get<IListFilesOperation>().ExecuteAsync(vam);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);
            _output.WriteLine($"Completed contents listing: found {matches.Count} matches in {varFiles.Count} var packages, {freeFiles.Count} free files.");
        }
    }
}