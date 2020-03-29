using System;
using System.IO.Abstractions;
using System.Threading.Tasks;

namespace Varbsorb
{
    class Program
    {
        static async Task Main(string vam, bool noop)
        {
            var output = new ConsoleOutput();
            var runtime = new Varbsorber(output, new FileSystem());
            await runtime.ExecuteAsync(vam, noop);
        }
    }
}
