using Common;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulxer.Drawing;
using Pulxer.History;
using Pulxer.Plugin;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer
{
    public class SchedulerService : IHostedService
    {
        private Scheduler _scheduler;
        private TickDispatcher _tickDisp;
        private ChartManagerCache _cmCache;
        private IServiceProvider _services;
        private int _downloadAllSecondsTimeout = 600; // sec
        private readonly ILogger _logger;
        private readonly PluginManager _pluginManager;

        public SchedulerService(TickDispatcher tickDisp, ChartManagerCache cmCache, ILogger<SchedulerService> logger, IConfiguration config, IServiceProvider services,
            Scheduler scheduler, PluginManager pm)
        {
            _tickDisp = tickDisp;
            _cmCache = cmCache;
            _services = services;
            _logger = logger;
            _scheduler = scheduler;
            _pluginManager = pm;

            var section = config.GetSection("Scheduler");
            var tasks = section.GetSection("tasks");
            foreach (var task in tasks.GetChildren())
            {
                int time;
                if (!int.TryParse(task["time"], out time)) continue;
                string action = task["action"].ToLower();
                if ((action == "initialize") || (action == "opensession")) _scheduler.AddItem(time, OpenSession);
                if (action == "closesession") _scheduler.AddItem(time, CloseSession);
            }

            var delaySection = section.GetSection("downloadall-timeout");
            int timeout;
            if (int.TryParse(delaySection.Value, out timeout))
            {
                _downloadAllSecondsTimeout = timeout;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start scheduler");
            return Task.Run(() => { _scheduler.Start(); });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop scheduler");
            return Task.Run(() => { _scheduler.Stop(); });
        }

        private void OpenSession()
        {
            _logger.LogInformation("Open session ...");

            DateTime today = DateTime.Today;

            // инициализация провайдера исторических данных
            _services.GetRequiredService<IHistoryProvider>().Initialize().Wait();

            // загрузка истории по вчерашний день включительно
            var cancellationTokenSource = new CancellationTokenSource(_downloadAllSecondsTimeout * 1000);
            using (var scope = _services.CreateScope())
            {
                var downloader = scope.ServiceProvider.GetRequiredService<HistoryDownloader>();
                downloader.DownloadAllAsync(today.AddDays(-1), false, null, cancellationTokenSource.Token).Wait();
            }

            // инициализация диспетчера тиков, начало новой сессии сегодняшним числом
            _tickDisp.Initialize(today);
            
            // очистка кэша объектов ChartManager, чтобы эти объекты создавались вновь уже для новой торговой сессии
            lock (_cmCache)
            {
                _cmCache.Clear();
            }

            // загрузка всех плагинов
            _pluginManager.LoadAllPlugins();

            _logger.LogInformation("Session opened.");
        }

        private void CloseSession()
        {
            _pluginManager.UnloadAllPlugins();
            _logger.LogInformation("Session closed.");
        }
    }
}
