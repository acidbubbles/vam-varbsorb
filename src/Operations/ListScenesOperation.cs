using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public interface IListScenesOperation : IOperation
    {
        Task<IList<SceneFile>> ExecuteAsync(string vam, IList<FreeFile> files);
    }

    public class ListScenesOperation : OperationBase, IListScenesOperation
    {
        private static readonly Regex _findFilesFastRegex = new Regex(
            ": ?\"(?<path>[^\"]+\\.[a-zA-Z]{3,6})\"",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture,
            TimeSpan.FromSeconds(10));
        private readonly IFileSystem _fs;

        public ListScenesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public async Task<IList<SceneFile>> ExecuteAsync(string vam, IList<FreeFile> files)
        {
            var scenes = new List<SceneFile>();
            var filesIndex = files.Where(f => f.Extension != ".json").ToDictionary(f => f.LocalPath, f => f);
            using (var reporter = new ProgressReporter<ListScenesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                var scenesScanned = 0;
                var potentialScenes = files.Where(f => f.Extension == ".json").ToList();
                foreach (var potentialScene in potentialScenes)
                {
                    var potentialSceneJson = await _fs.File.ReadAllTextAsync(potentialScene.Path);
                    var potentialSceneReferences = _findFilesFastRegex.Matches(potentialSceneJson).Where(m => m.Success).Select(m => m.Groups["path"].Value).ToList();
                    var sceneFolder = _fs.Path.GetDirectoryName(potentialScene.Path);
                    var references = new List<FreeFile>();
                    foreach (var reference in potentialSceneReferences)
                    {
                        if (reference.Contains(":")) continue;
                        if(reference.EndsWith(".cs")){
                            var temp = filesIndex.Where(k => k.Value.Extension == ".cs").ToList();
                        }
                        if(reference.EndsWith(".cslist") && reference.Contains("timeline")){
                            var temp = filesIndex.Where(k => k.Value.Extension == ".cslist").ToList();
                        }
                        if (filesIndex.TryGetValue(_fs.Path.GetFullPath(_fs.Path.Combine(sceneFolder, reference).RelativeTo(vam)), out var f1))
                        {
                            references.Add(f1);
                        }
                        else if (filesIndex.TryGetValue(_fs.Path.GetFullPath(_fs.Path.Combine(vam, reference).RelativeTo(vam)), out var f2))
                        {
                            references.Add(f2);
                        }
                    }
                    if (references.Count > 0)
                        scenes.Add(new SceneFile(potentialScene) { References = references });
                    reporter.Report(new ListScenesProgress { ScenesProcessed = ++scenesScanned, TotalScenes = potentialScenes.Count, Current = potentialScene.FilenameLower });
                }
            }

            _output.WriteLine($"Found {files.Count} files in the Saves and Custom folders.");

            return scenes;
        }

        public class ListScenesProgress
        {
            public int ScenesProcessed { get; set; }
            public int TotalScenes { get; set; }
            public string Current { get; set; }
        }

        private void ReportProgress(ListScenesProgress progress)
        {
            _output.WriteAndReset($"Parsing scenes... {progress.ScenesProcessed} / {progress.TotalScenes} ({progress.ScenesProcessed / (float)progress.TotalScenes * 100:0}%): {progress.Current}");
        }
    }
}