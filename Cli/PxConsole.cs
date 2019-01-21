using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
    public class PxConsole : IConsole
    {
        private const int WIDTH = 80;

        public void Write(string text)
        {
            Console.Write(text);
        }

        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        public void WriteError(string text)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n" + text);
            Console.ForegroundColor = color;
        }

        public void WriteSeparator()
        {
            Console.WriteLine("".PadRight(WIDTH, '_'));
            Console.WriteLine();
        }

        public void WriteTitle(string title)
        {
            int left = (WIDTH - title.Length) / 2;
            if (left < 0) left = 0;
            string paddedTitle = title.PadLeft(left + title.Length, '_').PadRight(WIDTH, '_');
            Console.WriteLine(paddedTitle);
            Console.WriteLine();
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
