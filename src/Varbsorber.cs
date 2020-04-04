using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class Varbsorber
    {
        public const int MaxWarnings = 20;

        private readonly IConsoleOutput _output;
        private readonly IOperationsFactory _operationsFactory;

        public Varbsorber(IConsoleOutput output, IOperationsFactory operationsFactory)
        {
            _output = output;
            _operationsFactory = operationsFactory;
        }

        public static IFilter BuildFilter(string vam, string[]? include, string[]? exclude)
        {
            return Filter.From(
                include?.Select(f => SanitizeFilterPath(vam, f)).ToArray(),
                exclude?.Select(f => SanitizeFilterPath(vam, f)).ToArray());
        }

        public async Task ExecuteAsync(string vam, string[]? include, string[]? exclude, bool verbose, bool warnings, bool noop)
        {
            vam = SanitizeVamRootFolder(vam);
            var filter = BuildFilter(vam, include, exclude);

            var sw = Stopwatch.StartNew();

            var varFiles = await _operationsFactory.Get<IListVarPackagesOperation>().ExecuteAsync(vam);
            var freeFiles = await _operationsFactory.Get<IListFilesOperation>().ExecuteAsync(vam);
            var scenes = await _operationsFactory.Get<IListScenesOperation>().ExecuteAsync(vam, freeFiles, filter, warnings);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);
            await _operationsFactory.Get<IUpdateSceneReferencesOperation>().ExecuteAsync(scenes, matches, noop);
            await _operationsFactory.Get<IDeleteMatchedFilesOperation>().ExecuteAsync(freeFiles, matches, filter, verbose, noop);
            await _operationsFactory.Get<IDeleteOrphanMorphFilesOperation>().ExecuteAsync(freeFiles, filter, verbose, noop);

            _output.WriteLine($"Cleanup complete in {sw.Elapsed.Seconds:0.00} seconds.");
        }

        private static string SanitizeVamRootFolder(string vam)
        {
            if (string.IsNullOrWhiteSpace(vam)) throw new VarbsorberException("The vam parameter is required (please specify the Virt-A-Mate installation folder)");
            if (vam.EndsWith('/') || vam.EndsWith('\\')) vam = vam[0..^1];
            return vam;
        }

        private static string SanitizeFilterPath(string vam, string f)
        {
            if (!Path.IsPathFullyQualified(f)) f = Path.Combine(vam, f);
            f = Path.GetFullPath(f);
            if (!f.StartsWith(vam)) throw new VarbsorberException($"Filter '{f}' is not within the vam folder");
            if (!f.StartsWith(Path.Combine(vam, "Saves"))) throw new VarbsorberException($"Filter '{f}' is not within the vam Saves folder");
            return f.Substring(vam.Length + 1);
        }
    }
}
