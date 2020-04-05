using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Varbsorb.Logging;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class UpdateSceneReferencesOperation : OperationBase, IUpdateSceneReferencesOperation
    {
        protected override string Name => "Update scene references";

        private readonly IFileSystem _fs;
        private readonly ILogger _logger;
        private int _processed;
        private int _updated;

        public UpdateSceneReferencesOperation(IConsoleOutput output, IFileSystem fs, ILogger logger)
            : base(output)
        {
            _fs = fs;
            _logger = logger;
        }

        public async Task ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches, ExecutionOptions execution)
        {
            var matchesIndex = matches.SelectMany(m => m.FreeFiles.SelectMany(ff => ff.SelfAndChildren()).Select(ff => (m, ff))).GroupBy(x => x.ff).ToDictionary(x => x.Key, x => x.Select(z => z.m).ToList());
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var processSceneBlock = new ActionBlock<SceneFile>(
                    scene => ProcessSceneAsync(scenes, execution, matchesIndex, reporter, scene),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = 4
                    });

                foreach (var scene in scenes)
                {
                    processSceneBlock.Post(scene);
                }

                processSceneBlock.Complete();
                await processSceneBlock.Completion;
            }

            if (execution == ExecutionOptions.Noop) Output.WriteLine($"Skipped updating {_updated} scenes since --noop was specified.");
            else Output.WriteLine($"Updated {_updated} scenes.");
        }

        private static FreeFilePackageMatch FindBestMatch(List<FreeFilePackageMatch> matches)
        {
            // Find the most recent and small package
            return matches
                .GroupBy(m => $"{m.Package.Name.Author}.{m.Package.Name.Name}")
                .Select(g => g.OrderByDescending(m => m.Package.Name.Version).First())
                .OrderBy(m => m.Package.Files.Count)
                .First();
        }

        private async Task ProcessSceneAsync(IList<SceneFile> scenes, ExecutionOptions execution, Dictionary<FreeFile, List<FreeFilePackageMatch>> matchesIndex, ProgressReporter<ProgressInfo> reporter, SceneFile scene)
        {
            reporter.Report(new ProgressInfo(Interlocked.Increment(ref _processed), scenes.Count, scene.File.LocalPath));

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
                Interlocked.Increment(ref _updated);
                if (execution == ExecutionOptions.Noop)
                {
                    _logger.Log($"[WRITE(NOOP)] {scene.File.Path}");
                }
                else
                {
                    _logger.Log($"[WRITE] {scene.File.Path}");
                    await _fs.File.WriteAllTextAsync(scene.File.Path, sb.ToString());
                }
            }
        }
    }

    public interface IUpdateSceneReferencesOperation : IOperation
    {
        Task ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches, ExecutionOptions execution);
    }
}
