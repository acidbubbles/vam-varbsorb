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
        private readonly IFileSystem _fs;

        public UpdateSceneReferencesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public async Task<IList<FreeFile>> ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches)
        {
            var freed = new List<FreeFile>();
            var matchesIndex = matches.SelectMany(m => m.FreeFiles.SelectMany(ff => ff.SelfAndChildren()).Select(ff => (m, ff))).ToDictionary(x => x.ff, x => (file: x.ff, match: x.m));
            using (var reporter = new ProgressReporter<UpdateScenesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                var scenesProcessed = 0;
                foreach (var scene in scenes)
                {
                    var sceneJsonTask = new Lazy<Task<StringBuilder>>(async () => new StringBuilder(await _fs.File.ReadAllTextAsync(scene.File.Path)));
                    foreach (var sceneRef in scene.References.OrderByDescending(r => r.Index))
                    {
                        if (matchesIndex.TryGetValue(sceneRef.File, out var match))
                        {
                            var sb = await sceneJsonTask.Value;
                            sb.Remove(sceneRef.Index, sceneRef.Length);
                            sb.Insert(sceneRef.Index, $"{match.match.Package.Name.Author}.{match.match.Package.Name.Name}.{match.match.Package.Name.Version}:/{match.match.PackageFile.LocalPath.Replace('\\', '/')}");
                        }
                    }
                    if (sceneJsonTask.IsValueCreated)
                    {
                        var sb = await sceneJsonTask.Value;
                        await _fs.File.WriteAllTextAsync(scene.File.Path, sb.ToString());
                    }

                    reporter.Report(new UpdateScenesProgress { ScenesProcessed = ++scenesProcessed, ScenesTotal = scenes.Count });
                }
            }

            Output.WriteLine($"Found {matches.Count} matching files.");

            return freed;
        }

        public class UpdateScenesProgress
        {
            public int ScenesTotal { get; set; }
            public int ScenesProcessed { get; set; }
        }

        private void ReportProgress(UpdateScenesProgress progress)
        {
            Output.WriteAndReset($"Updating scene references... {progress.ScenesProcessed} / {progress.ScenesTotal} ({progress.ScenesProcessed / (float)progress.ScenesTotal * 100:0}%)");
        }
    }

    public interface IUpdateSceneReferencesOperation : IOperation
    {
        Task<IList<FreeFile>> ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches);
    }
}
