using Common.Interfaces;
using System;
using System.Linq;

namespace Cli
{
    // этот объект будет удален вместе с LeechDA
    public class LeechCtrl
    {
        private readonly IConsole _console;
        private readonly IImportLeech _importLeech;

        public LeechCtrl(IConsole console, IImportLeech importLeech)
        {
            _console = console;
            _importLeech = importLeech;
        }

        public async void SyncAllTrades()
        {
            try
            {
                await _importLeech.SyncAllTradesAsync();
            }
            catch (AggregateException ex)
            {
                throw ex.Flatten();
            }
            _console.WriteLine("Синхронизация завершена");
        }
    }
}
