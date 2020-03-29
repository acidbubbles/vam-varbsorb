using System;
using System.Threading.Tasks;

namespace Varbsorb
{
    class Program
    {
        static async Task Main(string vam, bool noop)
        {
            var output = new ConsoleOutput(Console.Out);
            var runtime = new Varbsorber(output, vam, noop);
            await runtime.ExecuteAsync();
        }
    }
}
