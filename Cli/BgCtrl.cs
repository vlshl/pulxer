using Common.Interfaces;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Cli
{
    public class BgCtrl
    {
        private readonly IConsole _console;
        protected static BgTaskProgress _progress = null;
        protected static CancellationTokenSource _cancel;
        protected SyncContext _syncContext = new SyncContext();

        public BgCtrl(IConsole console)
        {
            _console = console;
        }

        public void ShowProgress(BgTaskProgress progress, int level = 0)
        {
            string spaces = new string('\t', level);
            _console.WriteLine(string.Format("{0}{1} : {2} : {3}%",
                spaces,
                progress.Name,
                progress.StateStr,
                progress.Percent.ToString("#0.00")));
            if (progress.Fault != null) _console.WriteLine(spaces + progress.Fault.ToString());
            foreach (var p in progress.Children)
            {
                ShowProgress(p, level + 1);
            }
        }
    }
}
