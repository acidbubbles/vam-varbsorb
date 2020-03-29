using System;
using System.IO;

namespace Varbsorb
{
    public interface IConsoleOutput
    {
        bool CursorVisible { get; set; }

        void WriteLine(string text);
        void WriteAndReset(string text);
    }

    public class ConsoleOutput : IConsoleOutput
    {

        public bool CursorVisible { get => Console.CursorVisible; set => Console.CursorVisible = value; }

        public ConsoleOutput()
        {
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteAndReset(string text)
        {
            Console.Write(text.PadRight(Console.BufferWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }
}