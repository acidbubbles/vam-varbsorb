using System.Threading.Tasks;

namespace Varbsorb.Logging
{
    public class NullLogger : ILogger
    {
        public bool Enabled => false;

        public Task DumpAsync()
        {
            return Task.CompletedTask;
        }

        public void Log(string message)
        {
        }
    }
}
