using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;
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

        public async Task ExecuteAsync(string vam, bool verbose, bool warnings, bool noop)
        {
            if (string.IsNullOrWhiteSpace(vam)) throw new VarbsorberException("The vam parameter is required (please specify the Virt-A-Mate installation folder)");
            if (vam.EndsWith('/') || vam.EndsWith('\\')) vam = vam[0..^1];

            var sw = Stopwatch.StartNew();

            var varFiles = await _operationsFactory.Get<IListVarPackagesOperation>().ExecuteAsync(vam);
            var freeFiles = await _operationsFactory.Get<IListFilesOperation>().ExecuteAsync(vam);
            var scenes = await _operationsFactory.Get<IListScenesOperation>().ExecuteAsync(vam, freeFiles);
            var matches = await _operationsFactory.Get<IMatchFilesToPackagesOperation>().ExecuteAsync(varFiles, freeFiles);
            var filesToDelete = new HashSet<FreeFile>();
            if (!noop)
            {
                var freed = await _operationsFactory.Get<IUpdateSceneReferencesOperation>().ExecuteAsync(scenes, matches);
                filesToDelete.UnionWith(freed);
            }

            _output.WriteLine("Scan complete. Files matching a var reference:");
            foreach (var match in matches.GroupBy(m => m.Package))
            {
                foreach (var file in match.SelectMany(m => m.FreeFiles).SelectMany(ff => ff.SelfAndChildren()))
                {
                    // TODO: Before we can use this, we must check for .wav files referenced by scripts for example.
                    // TODO: Only delete unused stuff in Saves, and delete in Custom only if a matching var exists.
                    // TODO: Delete empty folders
                    // TODO: Morph matches can be deleted directly
                    // TODO: When we delete a reference to a clothing item for example, make sure to delete all files that the var contains
                    filesToDelete.Add(file);
                    if (verbose) _output.WriteLine($"- {file.LocalPath} in {match.Key.Name.Filename} (used in {scenes.Count(s => s.References.Any(r => r.File == file))} scenes)");
                }
            }

            var errors = scenes.Where(s => s.Missing.Any()).ToList();
            if (errors.Count > 0 && warnings)
            {
                foreach (var scene in errors)
                {
                    _output.WriteLine($"{scene.Missing.Count} Errors in scene: {scene.File.LocalPath}");
                    foreach (var brokenRef in scene.Missing)
                    {
                        _output.WriteLine($"- {brokenRef}");
                    }
                }
            }

            _output.WriteLine($"Complete. Found {matches.Count} matches in {varFiles.Count} packages and {freeFiles.Count} files in {sw.Elapsed.Seconds:0.00} seconds. Estimated space saved: {filesToDelete.Sum(f => f.Size) / 1024f / 1024f:0.00}MB.");
        }
    }
}
