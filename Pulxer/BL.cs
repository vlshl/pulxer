using Common;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            services.AddTransient<IInstrumBL, InstrumBL>();
            services.AddTransient<IInsStoreBL, InsStoreBL>();
            services.AddTransient<IImportLeech, ImportLeech>();
            services.AddSingleton<IHistoryProvider, FinamHistoryProvider>();
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
        }
    }
}
