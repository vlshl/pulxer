using Common.Interfaces;
using Microsoft.Extensions.Logging;
using Pulxer.Leech;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer
{
    public class TickHistoryDownloader
    {
        private readonly LeechServerManager _lsm;
        private readonly ILogger<TickHistoryDownloader> _logger;
        private readonly ITickHistory _tickHistory;

        public TickHistoryDownloader(ITickHistory tickHistory, LeechServerManager lsm, ILogger<TickHistoryDownloader> logger)
        {
            _tickHistory = tickHistory;
            _lsm = lsm;
            _logger = logger;
        }

        public async Task<int> DownloadAsync()
        {
            var ls = _lsm.GetServer();
            if (ls == null)
            {
                _logger.LogError("LeechServer does not exist");
                return 0;
            }

            var srv = ls.CreateTickHistoryPipe().Result;
            if (srv == null)
            {
                _logger.LogError("TickHistoryPipeServer creation error");
                return 0;
            }

            int yearFrom = 2000;
            var lastDate = _tickHistory.GetLastDate();
            if (lastDate != null)
            {
                yearFrom = lastDate.Value.Year;
            }
            int yearTo = DateTime.Today.Year;

            int blobCount = 0;
            for (int y = yearFrom; y <= yearTo; ++y)
            {
                var remoteDates = await srv.GetDates(y);
                if (remoteDates == null)
                {
                    _logger.LogError("Get remote dates error");
                    continue;
                }
                if (!remoteDates.Any()) continue;

                var localDates = _tickHistory.GetDates(y).Select(r => r.ToString("yyyy-MM-dd")).ToList();

                foreach (var rDate in remoteDates)
                {
                    if (localDates.Contains(rDate)) continue;

                    var tickers = await srv.GetTickers(rDate);
                    if (tickers == null)
                    {
                        _logger.LogError("Get remote tickers error");
                        continue;
                    }
                    if (!tickers.Any()) continue;

                    foreach (var ticker in tickers)
                    {
                        var bytes = await srv.GetData(rDate, ticker);
                        if (bytes == null)
                        {
                            _logger.LogError("Get remote data error");
                            continue;
                        }
                        if (bytes.Length == 0) continue;

                        bool isSuccess = await _tickHistory.SaveTicksBlobAsync(DateTime.Parse(rDate), ticker, bytes);
                        if (isSuccess)
                        {
                            blobCount++;
                        }
                        else
                        {
                            _logger.LogError("Save ticks blob error");
                        }
                    }
                }
            }

            return blobCount;
        }
    }
}
