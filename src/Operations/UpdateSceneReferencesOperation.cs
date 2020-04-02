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

        public async Task<IList<FreeFile>> ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches)
        {
            var freed = new HashSet<FreeFile>();
            var matchesIndex = matches.SelectMany(m => m.FreeFiles.SelectMany(ff => ff.SelfAndChildren()).Select(ff => (m, ff))).ToDictionary(x => x.ff, x => (file: x.ff, match: x.m));
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
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
                            freed.Add(match.file);
                        }
                    }
                    if (sceneJsonTask.IsValueCreated)
                    {
                        var sb = await sceneJsonTask.Value;
                        await _fs.File.WriteAllTextAsync(scene.File.Path, sb.ToString());
                    }

                    reporter.Report(new ProgressInfo(++scenesProcessed, scenes.Count, scene.File.LocalPath));
                }
            }

            Output.WriteLine($"Updated {matches.Count} scenes.");

            return freed.ToList();
        }
    }

    public interface IUpdateSceneReferencesOperation : IOperation
    {
        Task<IList<FreeFile>> ExecuteAsync(IList<SceneFile> scenes, IList<FreeFilePackageMatch> matches);
    }
}
