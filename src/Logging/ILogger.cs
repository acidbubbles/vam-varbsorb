using System.Threading.Tasks;

namespace Varbsorb.Logging
{
    public interface ILogger
    {
        bool Enabled { get; }

        Task DumpAsync();
        void Log(string message);
    }
}
