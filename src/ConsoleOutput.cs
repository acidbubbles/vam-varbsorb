using System.IO;

namespace Varbsorb
{
    public interface IConsoleOutput
    {
        void WriteLine(string text);
    }

    public class ConsoleOutput : IConsoleOutput
    {
        private readonly TextWriter _output;

        public ConsoleOutput(TextWriter output)
        {
            _output = output;
        }

        public void WriteLine(string text)
        {
            _output.WriteLine(text);
        }
    }
}