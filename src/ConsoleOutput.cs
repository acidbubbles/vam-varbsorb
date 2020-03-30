using System;

namespace Varbsorb
{
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
            var width = Console.BufferWidth - 1;
            if (text.Length > width) text = text.Substring(0, width);
            else if (text.Length < width) text = text.PadRight(width);
            Console.Write(text);
            Console.SetCursorPosition(0, Console.CursorTop);
        }
    }

    public interface IConsoleOutput
    {
        bool CursorVisible { get; set; }

        void WriteLine(string text);
        void WriteAndReset(string text);
    }
}
