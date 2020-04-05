using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Varbsorb.Models;

namespace Varbsorb.Operations
{
    public class ListFilesOperation : OperationBase, IListFilesOperation
    {
        protected override string Name => "Scan files";
        private readonly IFileSystem _fs;
        private int _scanned = 0;

        public ListFilesOperation(IConsoleOutput output, IFileSystem fs)
            : base(output)
        {
            _fs = fs;
        }

        public async Task<IList<FreeFile>> ExecuteAsync(string vam)
        {
            var files = new List<FreeFile>();
            using (var reporter = new ProgressReporter<ProgressInfo>(StartProgress, ReportProgress, CompleteProgress))
            {
                files.AddRange(ScanFolder(reporter, vam, "Custom"));
                files.AddRange(ScanFolder(reporter, vam, "Saves"));
                await GroupCslistRefs(vam, files);
                GroupMorphsVmi(files);
            }

            Output.WriteLine($"Scanned {files.Count} files in the Saves and Custom folders.");

            return files;
        }

        private IEnumerable<FreeFile> ScanFolder(IProgress<ProgressInfo> reporter, string vam, string folder)
        {
            return _fs.Directory
                .EnumerateFiles(_fs.Path.Combine(vam, folder), "*.*", SearchOption.AllDirectories)
                // Folders starting with a dot will not be cleaned, it would be better to avoid browsing but hey.
                .Where(f => !f.Contains(@"\."))
                .Select(f => new FreeFile(f, f.RelativeTo(vam)))
                .Tap(f => reporter.Report(new ProgressInfo(Interlocked.Increment(ref _scanned), 0, f.LocalPath)))
                .Tap(f =>
                {
                    if (f.Extension == ".exe")
                    {
                        throw new SecurityException($"An executable file was found in your '{vam}' folder: '{f.Path}'");
                    }
                });
        }

        private async Task GroupCslistRefs(string vam, List<FreeFile> files)
        {
            var filesMovedAsChildren = new List<FreeFile>();
            var filesIndex = files.Where(f => f.Extension == ".cs").ToDictionary(f => f.Path, f => f);
            foreach (var cslist in files.Where(f => f.Extension == ".cslist"))
            {
                cslist.Children = new List<FreeFile>();
                var cslistFolder = _fs.Path.GetDirectoryName(cslist.Path);
                foreach (var cslistRef in await _fs.File.ReadAllLinesAsync(cslist.Path))
                {
                    if (string.IsNullOrWhiteSpace(cslistRef)) continue;
                    if (filesIndex.TryGetValue(_fs.Path.GetFullPath(_fs.Path.Combine(cslistFolder, cslistRef)), out var f1))
                    {
                        cslist.Children.Add(f1);
                        filesMovedAsChildren.Add(f1);
                    }
                    else if (filesIndex.TryGetValue(_fs.Path.GetFullPath(_fs.Path.Combine(vam, cslistRef)), out var f2))
                    {
                        cslist.Children.Add(f2);
                        filesMovedAsChildren.Add(f2);
                    }
                }
            }
            filesMovedAsChildren.ForEach(f => files.Remove(f));
        }

        private void GroupMorphsVmi(List<FreeFile> files)
        {
            var filesMovedAsChildren = new List<FreeFile>();
            var pairs = files
                .Where(f => f.Extension == ".vmi" || f.Extension == ".vmb")
                .Select(f => (basePath: f.LocalPath.Substring(0, f.LocalPath.Length - f.Extension.Length), file: f))
                .GroupBy(x => x.basePath)
                .Where(g => g.Count() == 2)
                .Select(g => (vmi: g.Single(f => f.file.Extension == ".vmi").file, vmb: g.Single(f => f.file.Extension == ".vmb").file));
            foreach (var (vmi, vmb) in pairs)
            {
                vmi.Children = new List<FreeFile> { vmb };
                filesMovedAsChildren.Add(vmb);
            }
            filesMovedAsChildren.ForEach(f => files.Remove(f));
        }
    }

    public interface IListFilesOperation : IOperation
    {
        Task<IList<FreeFile>> ExecuteAsync(string vam);
    }
}
