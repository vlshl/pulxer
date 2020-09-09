using Common.Interfaces;
using System;
using System.Linq;

namespace Cli
{
    // этот объект будет удален вместе с LeechDA
    public class LeechCtrl
    {
        private readonly IConsole _console;
        private readonly ILeechDA _leechDA;
        private readonly IImportLeech _importLeech;

        public LeechCtrl(IConsole console, ILeechDA leechDA, IImportLeech importLeech)
        {
            _console = console;
            _leechDA = leechDA;
            _importLeech = importLeech;
        }

        public void ListLeechInstrum()
        {
            var instrums = _leechDA.GetInstrumList();

            _console.WriteLine("InsID\tTicker\tShortName");
            _console.WriteSeparator();
            foreach (var instrum in instrums)
            {
                _console.WriteLine(string.Format("{0}\t{1}\t{2}",
                    instrum.InsID.ToString(),
                    instrum.Ticker,
                    instrum.ShortName));
            }
            _console.WriteLine(string.Format("Leech instrums: {0}", instrums.Count().ToString()));
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
