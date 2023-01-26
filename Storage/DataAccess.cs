using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Storage
{
    public static class DataAccess
    {
        public static void ConfigureServices(IServiceCollection services, string connectionString)
        {
            services.AddTransient<IInstrumDA, InstrumDA>();
            services.AddTransient<IInsStoreDA, InsStoreDA>();
            services.AddTransient<ITickSourceDA, TickSourceDA>();
            services.AddTransient<ITestConfigDA, TestConfigDA>();
            services.AddTransient<IAccountDA, AccountDA>();
            services.AddTransient<ITickHistoryDA, TickHistoryDA>();
            services.AddTransient<IReplicationDA, ReplicationDA>();
            services.AddTransient<IPositionDA, PositionDA>();
            services.AddTransient<IUserDA, UserDA>();
            services.AddTransient<IRepositoryDA, RepositoryDA>();
            services.AddTransient<IChartDA, ChartDA>();
            services.AddTransient<IDeviceDA, DeviceDA>();

            services.AddDbContext<DaContext>(options => options.UseNpgsql(connectionString));
        }
    }
}
