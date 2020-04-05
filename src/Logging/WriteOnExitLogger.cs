using System.Collections.Concurrent;
using System.IO;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Varbsorb.Logging
{
    public class WriteOnExitLogger : ILogger
    {
        public bool Enabled => true;

        private readonly IFileSystem _fs;
        private readonly string _path;
        private readonly ConcurrentQueue<string> _lines = new ConcurrentQueue<string>();

        public WriteOnExitLogger(IFileSystem fs, string path)
        {
            _fs = fs;
            _path = path;
        }

        public void Log(string message)
        {
            _lines.Enqueue(message);
        }

        public async Task DumpAsync()
        {
            using var stream = _fs.File.OpenWrite(_path);
            using var writer = new StreamWriter(stream);
            while (_lines.TryDequeue(out var line))
                await writer.WriteLineAsync(line);
        }
    }
}
