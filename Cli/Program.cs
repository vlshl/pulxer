using Common;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var app = new PxCliApplication();
            app.Run();
        }
    }
}
