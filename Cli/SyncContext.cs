using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
    public class SyncContext : ISyncContext
    {
        public void RunAsync(Action action)
        {
            action.Invoke();
        }
    }
}
