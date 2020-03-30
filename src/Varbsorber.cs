using System.Linq;
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
            if (vam.EndsWith('/') || vam.EndsWith('\\')) vam = vam[0..^1];

            var varFiles = await _operationsFactory.Get<IListVarPackagesOperation>().ExecuteAsync(vam);
            var freeFiles = await _operationsFactory.Get<IListFilesOperation>().ExecuteAsync(vam);
            var scenes = await _operationsFactory.Get<IListScenesOperation>().ExecuteAsync(vam, freeFiles);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);
            _output.WriteLine($"Completed contents listing: found {matches.Count} matches in {varFiles.Count} var packages, {freeFiles.Count} free files.");
            foreach (var match in matches.GroupBy(m => m.Package))
            {
                var matchedFiles = match.SelectMany(m => m.FreeFiles).ToList();
                _output.WriteLine($"Package {match.Key} matched {matchedFiles.Count()} files used in {scenes.Count(s => s.References.Any(r => matchedFiles.Contains(r)))} scenes.");
            }
        }
    }
}
