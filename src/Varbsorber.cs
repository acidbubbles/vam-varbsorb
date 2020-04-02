using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;
using Varbsorb.Operations;

namespace Varbsorb
{
    public class Varbsorber
    {
        public const int MaxWarnings = 20;
        public const int MaxVerbose = 100;

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
            var scenes = await _operationsFactory.Get<IListScenesOperation>().ExecuteAsync(vam, freeFiles, filter);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);
            var filesToDelete = await _operationsFactory.Get<IListUnusedFilesOperation>().ExecuteAsync(matches, filter);
            if (!noop)
            {
                await _operationsFactory.Get<IUpdateSceneReferencesOperation>().ExecuteAsync(scenes, matches);
            }

            PrintFilesToDelete(verbose, filesToDelete);
            PrintSceneWarnings(warnings, scenes);

            _output.WriteLine($"Complete. Found {matches.Count} matches in {varFiles.Count} packages and {freeFiles.Count} files in {sw.Elapsed.Seconds:0.00} seconds. {filesToDelete.Count} files can be deleted. Estimated space saved: {filesToDelete.Sum(f => f.Size) / 1024f / 1024f:0.00}MB.");
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

        private void PrintFilesToDelete(bool verbose, ISet<FreeFile> filesToDelete)
        {
            if (verbose)
            {
                _output.WriteLine("Files to be deleted:");
                foreach (var file in filesToDelete.Take(MaxVerbose))
                    _output.WriteLine($"- {file.LocalPath}");
            }
        }

        private void PrintSceneWarnings(bool warnings, IList<SceneFile> scenes)
        {
            var errors = scenes.Where(s => s.Missing.Any()).ToList();
            if (errors.Count > 0 && warnings)
            {
                foreach (var scene in errors.Take(MaxWarnings))
                {
                    _output.WriteLine($"{scene.Missing.Count} Errors in scene: {scene.File.LocalPath}");
                    foreach (var brokenRef in scene.Missing.Distinct())
                    {
                        _output.WriteLine($"- {brokenRef}");
                    }
                }
            }
        }
    }
}
