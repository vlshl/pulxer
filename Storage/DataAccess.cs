using Common;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage
{
    public static class DataAccess
    {
        public static void ConfigureServices(IServiceCollection services, 
            string pulxerConnectionString, string leechConnectionString)
        {
            services.AddTransient<IInstrumDA, InstrumDA>();
            services.AddTransient<IInsStoreDA, InsStoreDA>();
            services.AddTransient<ILeechDA, LeechDA>();
            services.AddTransient<ITickSourceDA, TickSourceDA>();
            services.AddTransient<ITestConfigDA, TestConfigDA>();
            services.AddTransient<IAccountDA, AccountDA>();
            services.AddTransient<ITickHistoryDA, TickHistoryDA>();
            services.AddTransient<IReplicationDA, ReplicationDA>();
            services.AddTransient<IPositionDA, PositionDA>();
            services.AddTransient<IUserDA, UserDA>();
            services.AddTransient<IRepositoryDA, RepositoryDA>();
            services.AddTransient<IChartDA, ChartDA>();

            services.AddDbContext<DaContext>(options => options.UseNpgsql(pulxerConnectionString));
            services.AddDbContext<LeechDAContext>(options => options.UseSqlite(leechConnectionString));
        }
    }
}
