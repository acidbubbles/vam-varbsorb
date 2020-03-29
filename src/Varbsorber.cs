using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Varbsorb
{
    public class Varbsorber
    {
        private readonly IConsoleOutput _output;
        private readonly IFileSystem _fs;
        private readonly string _vam;
        private readonly bool _noop;

        public Varbsorber(IConsoleOutput output, IFileSystem fs)
        {
            _output = output;
            _fs = fs;
        }

        public Task ExecuteAsync(string vam, bool noop)
        {
            _output.WriteLine($"Scanning {_vam} (Custom and Saves folders)");

            var files = new List<string>();
            using (var reporter = new ProgressReporter<ListFilesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                var counter = 0;
                files.AddRange(_fs.Directory.GetFiles(_fs.Path.Combine(vam, "Custom"), "*.*", SearchOption.AllDirectories).Tap(f => reporter.Report(new ListFilesProgress { Folder = _fs.Path.GetDirectoryName(f), Files = ++counter })));
                files.AddRange(_fs.Directory.GetFiles(_fs.Path.Combine(vam, "Saves"), "*.*", SearchOption.AllDirectories).Tap(f => reporter.Report(new ListFilesProgress { Folder = _fs.Path.GetDirectoryName(f), Files = ++counter })));
            }

            _output.WriteLine($"Scan complete, {files.Count} files found");

            return Task.CompletedTask;
        }

        public class ListFilesProgress
        {
            public int Files { get; set; }
            public string Folder { get; set; }
        }

        private void StartProgress()
        {
            _output.CursorVisible = false;
        }

        private void ReportProgress(ListFilesProgress progress)
        {
            _output.WriteAndReset($"Scanning... {progress.Files} discovered: {progress.Folder}");
        }

        private void CompleteProgress()
        {
            _output.CursorVisible = false;
        }
    }
}