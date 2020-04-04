using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class UpdateSceneReferencesOperation : OperationBase, IUpdateSceneReferencesOperation
    {
        protected override string Name => "Update scene references";

        private readonly IFileSystem _fs;

        public UpdateSceneReferencesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public async Task ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches, ExecutionOptions execution)
        {
            var scenesProcessed = 0;
            var matchesIndex = matches.SelectMany(m => m.FreeFiles.SelectMany(ff => ff.SelfAndChildren()).Select(ff => (m, ff))).GroupBy(x => x.ff).ToDictionary(x => x.Key, x => x.Select(z => z.m).ToList());
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                foreach (var scene in scenes)
                {
                    var sceneJsonTask = new Lazy<Task<StringBuilder>>(async () => new StringBuilder(await _fs.File.ReadAllTextAsync(scene.File.Path)));
                    foreach (var sceneRef in scene.References.OrderByDescending(r => r.Index))
                    {
                        if (matchesIndex.TryGetValue(sceneRef.File, out var fileMatches))
                        {
                            var match = FindBestMatch(fileMatches);
                            var sb = await sceneJsonTask.Value;
                            sb.Remove(sceneRef.Index, sceneRef.Length);
                            sb.Insert(sceneRef.Index, $"{match.Package.Name.Author}.{match.Package.Name.Name}.{match.Package.Name.Version}:/{match.PackageFile.LocalPath.Replace('\\', '/')}");
                        }
                    }
                    if (sceneJsonTask.IsValueCreated)
                    {
                        var sb = await sceneJsonTask.Value;
                        if (execution != ExecutionOptions.Noop)
                            await _fs.File.WriteAllTextAsync(scene.File.Path, sb.ToString());
                    }

                    reporter.Report(new ProgressInfo(++scenesProcessed, scenes.Count, scene.File.LocalPath));
                }
            }

            if (execution == ExecutionOptions.Noop) Output.WriteLine($"Skipped updating {scenesProcessed} scenes since --noop was specified.");
            else Output.WriteLine($"Updated {scenesProcessed} scenes.");
        }

        private FreeFilePackageMatch FindBestMatch(List<FreeFilePackageMatch> matches)
        {
            // Find the most recent and small package
            return matches
                .GroupBy(m => $"{m.Package.Name.Author}.{m.Package.Name.Name}")
                .Select(g => g.OrderByDescending(m => m.Package.Name.Version).First())
                .OrderBy(m => m.Package.Files.Count)
                .First();
        }
    }

    public interface IUpdateSceneReferencesOperation : IOperation
    {
        Task ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches, ExecutionOptions execution);
    }
}
