using Common;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Pulxer.Drawing;
using Pulxer.History;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer
{
    public class SchedulerService : IHostedService
    {
        private Scheduler _scheduler;
        private ITickDispatcher _tickDisp;
        private ChartManagerCache _cmCache;
        private IServiceProvider _services;
        private int _downloadAllTimeout = 600; // sec

        public SchedulerService(ITickDispatcher tickDisp, ChartManagerCache cmCache, ILogger logger, IConfiguration config, IServiceProvider services)
        {
            _tickDisp = tickDisp;
            _cmCache = cmCache;
            _services = services;

            _scheduler = new Scheduler(logger);
            var section = config.GetSection("Scheduler");
            if (section != null)
            {
                var tasks = section.GetSection("tasks");
                if (tasks != null)
                {
                    foreach(var task in tasks.GetChildren())
                    {
                        int time;
                        if (!int.TryParse(task["time"], out time)) continue;
                        string action = task["action"];
                        if (action.ToLower() == "initialize") _scheduler.AddItem(time, OpenSession);
                    }
                }

                var delaySection = section.GetSection("downloadall-timeout");
                int timeout;
                if (int.TryParse(delaySection.Value, out timeout))
                {
                    _downloadAllTimeout = timeout;
                }
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => { _scheduler.Start(); });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => { _scheduler.Stop(); });
        }

        private void OpenSession()
        {
            DateTime today = DateTime.Today;

            _services.GetRequiredService<IHistoryProvider>().Initialize().Wait();
            _tickDisp.Initialize();
            lock (_cmCache)
            {
                _cmCache.Clear();
            }

            var cancellationTokenSource = new CancellationTokenSource(_downloadAllTimeout * 1000);
            using (var scope = _services.CreateScope())
            {
                var downloader = scope.ServiceProvider.GetRequiredService<HistoryDownloader>();
                downloader.DownloadAllAsync(today.AddDays(-1), false, null, cancellationTokenSource.Token).Wait();
            }
        }
    }
}
