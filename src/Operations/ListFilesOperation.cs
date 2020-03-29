using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

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

        public Task<IList<FreeFile>> ExecuteAsync(string vam)
        {
            var files = new List<FreeFile>();
            using (var reporter = new ProgressReporter<ListFilesProgress>(StartProgress, ReportProgress, CompleteProgress))
            {
                var counter = 0;
                files.AddRange(_fs.Directory.GetFiles(_fs.Path.Combine(vam, "Custom"), "*.*", SearchOption.AllDirectories).Select(f => new FreeFile(f)).Tap(f => reporter.Report(new ListFilesProgress { Folder = _fs.Path.GetDirectoryName(f.Path), Files = ++counter })));
                files.AddRange(_fs.Directory.GetFiles(_fs.Path.Combine(vam, "Saves"), "*.*", SearchOption.AllDirectories).Select(f => new FreeFile(f)).Tap(f => reporter.Report(new ListFilesProgress { Folder = _fs.Path.GetDirectoryName(f.Path), Files = ++counter })));
            }

            _output.WriteLine($"Found {files.Count} files in the Saves and Custom folders.");

            return Task.FromResult((IList<FreeFile>)files);
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