using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class ListScenesOperation : OperationBase, IListScenesOperation
    {
        protected override string Name => "Scan scene references";

        private static readonly Regex _findFilesFastRegex = new Regex(
            "\"(assetUrl|audioClip|url|uid|sceneFilePath|plugin#[0-9+]|act1Target[0-9]+ValueName)\" ?: ?\"(?<path>[^\"]+\\.[a-zA-Z]{2,6})\"",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture,
            TimeSpan.FromSeconds(10));

        private readonly IFileSystem _fs;
        private readonly ConcurrentBag<SceneFile> _scenes = new ConcurrentBag<SceneFile>();
        private int _scanned = 0;

        private int _warningsLeft = 100;

        public ListScenesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public async Task<IList<SceneFile>> ExecuteAsync(string vam, IList<FreeFile> files, IFilter filter, ErrorReportingOptions warnings)
        {
            var filesIndex = new ConcurrentDictionary<string, FreeFile>(files.ToDictionary(f => f.Path, f => f));
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                var potentialScenes = files
                    .Where(f => f.Extension == ".json")
                    .Where(f => !filter.IsFiltered(f.LocalPath))
                    .ToList();

                var scanSceneBlock = new ActionBlock<FreeFile>(
                    s => ScanSceneAsync(vam, s, potentialScenes.Count, filesIndex, reporter),
                    new ExecutionDataflowBlockOptions
                    {
                        MaxDegreeOfParallelism = 4
                    });

                foreach (var potentialScene in potentialScenes)
                {
                    scanSceneBlock.Post(potentialScene);
                }

                scanSceneBlock.Complete();
                await scanSceneBlock.Completion;
            }

            Output.WriteLine($"Scanned {_scanned} scenes.");

            var scenes = _scenes.OrderBy(s => s.File.LocalPath).ToList();

            if (warnings == ErrorReportingOptions.ShowWarnings)
                PrintWarnings(scenes);

            return scenes;
        }

        private static string MigrateLegacyPaths(string refPath)
        {
            if (refPath.StartsWith(@"Saves\Scripts\", StringComparison.OrdinalIgnoreCase)) return @"Custom\Scripts\" + refPath.Substring(@"Saves\Scripts\".Length);
            if (refPath.StartsWith(@"Saves\Assets\", StringComparison.OrdinalIgnoreCase)) return @"Custom\Assets\" + refPath.Substring(@"Saves\Assets\".Length);
            if (refPath.StartsWith(@"Import\morphs\", StringComparison.OrdinalIgnoreCase)) return @"Custom\Atom\Person\Morphs\" + refPath.Substring(@"Import\morphs\".Length);
            if (refPath.StartsWith(@"Textures\", StringComparison.OrdinalIgnoreCase)) return @"Custom\Atom\Person\Textures\" + refPath.Substring(@"Textures\".Length);
            return refPath;
        }

        private void PrintWarnings(List<SceneFile> scenes)
        {
            foreach (var scene in scenes.Where(s => s.Missing.Count > 0))
            {
                var missing = scene.Missing;
                if (_warningsLeft > 0)
                {
                    Output.WriteLine($"{missing.Count} missing references in scene {scene.File.LocalPath}");
                    foreach (var brokenRef in missing.Distinct())
                    {
                        Output.WriteLine($"- {brokenRef}");
                    }
                    if (--_warningsLeft == 0)
                    {
                        Output.WriteLine("Too many scene errors. Further missing references will not be printed.");
                    }
                }
            }
        }

        private async Task ScanSceneAsync(string vam, FreeFile potentialScene, int potentialScenesCount, ConcurrentDictionary<string, FreeFile> filesIndex, ProgressReporter<ProgressInfo> reporter)
        {
            reporter.Report(new ProgressInfo(Interlocked.Increment(ref _scanned), potentialScenesCount, potentialScene.LocalPath));

            var potentialSceneJson = await _fs.File.ReadAllTextAsync(potentialScene.Path);
            var potentialSceneReferences = _findFilesFastRegex.Matches(potentialSceneJson).Where(m => m.Success).Select(m => m.Groups["path"]);
            var sceneFolder = _fs.Path.GetDirectoryName(potentialScene.Path);
            var references = new List<SceneReference>();
            var missing = new HashSet<string>();
            foreach (var reference in potentialSceneReferences)
            {
                if (!reference.Success) continue;
                var refPath = reference.Value;
                if (refPath.Contains(":")) continue;
                refPath = refPath.NormalizePathSeparators();
                refPath = MigrateLegacyPaths(refPath);
                if (filesIndex.TryGetValue(_fs.Path.GetFullPath(_fs.Path.Combine(sceneFolder, refPath)), out var f1))
                {
                    references.Add(new SceneReference(f1, reference.Index, reference.Length));
                }
                else if (filesIndex.TryGetValue(_fs.Path.GetFullPath(_fs.Path.Combine(vam, refPath)), out var f2))
                {
                    references.Add(new SceneReference(f2, reference.Index, reference.Length));
                }
                else
                {
                    missing.Add(refPath);
                }
            }
            var item = new SceneFile(potentialScene, references, missing.ToList());
            if (references.Count > 0)
                _scenes.Add(item);
        }
    }

    public interface IListScenesOperation : IOperation
    {
        Task<IList<SceneFile>> ExecuteAsync(string vam, IList<FreeFile> files, IFilter filter, ErrorReportingOptions warnings);
    }
}
