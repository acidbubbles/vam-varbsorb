using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public interface IListFilesOperation : IOperation
    {
        Task<IList<FreeFile>> ExecuteAsync(string vam);
    }

    public class ListFilesOperation : OperationBase, IListFilesOperation
    {
        private readonly IFileSystem _fs;

        public ListFilesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public async Task<IList<FreeFile>> ExecuteAsync(string vam)
        {
            var files = new List<FreeFile>();
            using (var reporter = new ProgressReporter<ListFilesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                var counter = 0;
                files.AddRange(_fs.Directory
                    .GetFiles(_fs.Path.Combine(vam, "Custom"), "*.*", SearchOption.AllDirectories)
                    .Select(f => new FreeFile(f, f.RelativeTo(vam), _fs.Path.GetFileName(f).ToLowerInvariant(), _fs.Path.GetExtension(f).ToLowerInvariant()))
                    .Tap(f => reporter.Report(new ListFilesProgress { Folder = _fs.Path.GetDirectoryName(f.Path), Files = ++counter }))
                );
                files.AddRange(_fs.Directory
                    .GetFiles(_fs.Path.Combine(vam, "Saves"), "*.*", SearchOption.AllDirectories)
                    .Select(f => new FreeFile(f, f.RelativeTo(vam), _fs.Path.GetFileName(f).ToLowerInvariant(), _fs.Path.GetExtension(f).ToLowerInvariant()))
                    .Tap(f => reporter.Report(new ListFilesProgress { Folder = _fs.Path.GetDirectoryName(f.Path), Files = ++counter }))
                );

                var filesToRemove = new List<FreeFile>();
                var filesIndex = files.Where(f => f.Extension == ".cs").ToDictionary(f => f.Path, f => f);
                foreach (var cslist in files.Where(f => f.Extension == ".cslist"))
                {
                    cslist.Children = new List<FreeFile>();
                    var cslistFolder = _fs.Path.GetDirectoryName(cslist.Path);
                    foreach (var cslistRef in await _fs.File.ReadAllLinesAsync(cslist.Path))
                    {
                        if (string.IsNullOrWhiteSpace(cslistRef)) continue;
                        {
                            var fromRelativePath = _fs.Path.GetFullPath(_fs.Path.Combine(cslistFolder, cslistRef));
                            if (filesIndex.TryGetValue(fromRelativePath, out var found))
                            {
                                cslist.Children.Add(found);
                                filesToRemove.Add(found);
                            }
                        }
                        {
                            var fromVamPath = _fs.Path.GetFullPath(_fs.Path.Combine(vam, cslistRef));
                            if (filesIndex.TryGetValue(fromVamPath, out var found))
                            {
                                cslist.Children.Add(found);
                                filesToRemove.Add(found);
                            }
                        }
                    }
                }
                filesToRemove.ForEach(f => files.Remove(f));
            }

            _output.WriteLine($"Found {files.Count} files in the Saves and Custom folders.");

            return files;
        }

        public class ListFilesProgress
        {
            public int Files { get; set; }
            public string Folder { get; set; }
        }

        private void ReportProgress(ListFilesProgress progress)
        {
            _output.WriteAndReset($"Scanning... {progress.Files} discovered: {progress.Folder}");
        }
    }
}