using Common;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Pulxer.History;
using Pulxer.HistoryProvider;
using Storage;

namespace Pulxer
{
    public static class BL
    {
        public static void ConfigureServices(IServiceCollection services, 
            string pulxerConnectionString, string leechConnectionString)
        {
            DataAccess.ConfigureServices(services, pulxerConnectionString, leechConnectionString);

            services.AddSingleton<ILogger, Logger>();
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
            services.AddTransient<IImportLeech, ImportLeech>();
            services.AddTransient<IPositionBL, PositionBL>();
            services.AddTransient<HistoryDownloader>();
            services.AddTransient<ISyncBL, SyncBL>();
            services.AddTransient<IUserBL, UserBL>();
            services.AddTransient<IRepositoryBL, RepositoryBL>();
        }
    }
}
