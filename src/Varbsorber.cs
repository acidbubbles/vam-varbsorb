using System.Threading.Tasks;

namespace Varbsorb
{
    public class Varbsorber
    {
        private readonly IConsoleOutput _output;
        private readonly string _vam;
        private readonly bool _noop;

        public Varbsorber(IConsoleOutput output, string vam, bool noop)
        {
            _output = output;
            _vam = vam;
            _noop = noop;
        }

        public Task ExecuteAsync()
        {
            _output.WriteLine($"Processing {_vam}");
            return Task.CompletedTask;
        }
    }
}