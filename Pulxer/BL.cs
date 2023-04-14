using Common;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Platform;
using Pulxer.Drawing;
using Pulxer.History;
using Pulxer.HistoryProvider;
using Pulxer.Leech;
using Pulxer.Plugin;
using Storage;

namespace Pulxer
{
    public static class BL
    {
        public static void ConfigureServices(IServiceCollection services, IConfiguration config, string connectionString)
        {
            DataAccess.ConfigureServices(services, connectionString);

            services.AddSingleton<IConfig>(new Config(config)); 
            services.AddSingleton<LeechServerManager>();
            services.AddSingleton<ChartManagerCache>();
            services.AddSingleton<IHistoryProvider, FinamHistoryProvider>();
            services.AddSingleton<ITickSubscribe, TickDispatcher>();
            services.AddSingleton<TickDispatcher>();
            services.AddSingleton<Scheduler>();
            services.AddSingleton<TickProvider>();
            services.AddSingleton<InstrumCache>();
            services.AddSingleton<PluginManager>();
            
            services.AddTransient<IPluginPlatform, PluginPlatform>();
            services.AddTransient<HistoryDownloader>();
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
            services.AddTransient<ISyncBL, SyncBL>();
            services.AddTransient<IUserBL, UserBL>();
            services.AddTransient<IRepositoryBL, RepositoryBL>();
            services.AddTransient<IDependencyManager, DependencyManager>();
            services.AddTransient<ChartSystem>();
            services.AddTransient<ISettingsBL, SettingsBL>();

            services.AddHostedService<SchedulerService>();
        }
    }
}
