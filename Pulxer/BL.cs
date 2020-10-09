using Common;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pulxer.Drawing;
using Pulxer.History;
using Pulxer.HistoryProvider;
using Pulxer.Leech;
using Storage;

namespace Pulxer
{
    public static class BL
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration config,
            string pulxerConnectionString, string leechConnectionString)
        {
            DataAccess.ConfigureServices(services, pulxerConnectionString, leechConnectionString);

            var confSection = config.GetSection("Config");
            services.AddSingleton<IConfig>(new Config(confSection)); 
            services.AddSingleton<ILogger, Logger>();
            services.AddSingleton<LeechServerManager>();
            services.AddSingleton<ChartManagerCache>();
            services.AddSingleton<IHistoryProvider, FinamHistoryProvider>();
            services.AddSingleton<ITickDispatcher, TickDispatcher>();

            services.AddTransient<IInstrumBL, InstrumBL>();
            services.AddTransient<IInsStoreBL, InsStoreBL>();
            services.AddTransient<IImportLeech, ImportLeech>();
            services.AddTransient<IRequester, Requester>();
            services.AddTransient<ITestConfigBL, TestConfigBL>();
            services.AddTransient<ITickSourceBL, TickSourceBL>();
            services.AddTransient<IAccountBL, AccountBL>();
            services.AddTransient<ITickHistoryBL, TickHistoryBL>();
            services.AddTransient<IReplicationBL, ReplicationBL>();
            services.AddTransient<IPositionBL, PositionBL>();
            services.AddTransient<HistoryDownloader>();
            services.AddTransient<ISyncBL, SyncBL>();
            services.AddTransient<IUserBL, UserBL>();
            services.AddTransient<IRepositoryBL, RepositoryBL>();
            services.AddTransient<IDependencyManager, DependencyManager>();
            services.AddTransient<ChartSystem>();
        }
    }
}
