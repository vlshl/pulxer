using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
    public interface IExecutor
    {
        IExecutor Execute(string cmd, List<string> args);
        string GetPrefix();
    }
}
