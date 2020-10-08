using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Platform;
using Storage.DbModel;

namespace Storage
{
    public class DaContext : DbContext
    {
        public DaContext(DbContextOptions<DaContext> options) : base(options)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public DbSet<Instrum> Instrum { get; set; }
        public DbSet<InsStore> InsStore { get; set; }
        public DbSet<DbBarHistory> DbBarHistory { get; set; }
        public DbSet<DbTickHistory> DbTickHistory { get; set; }
        public DbSet<InsStorePeriods> InsStorePeriods { get; set; }
        public DbSet<InsStoreFreeDays> InsStoreFreeDays { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<Cash> Cash { get; set; }
        public DbSet<Holding> Holding { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<StopOrder> StopOrder { get; set; }
        public DbSet<Trade> Trade { get; set; }
        public DbSet<DbTickSource> TickSource { get; set; }
        public DbSet<DbTestConfig> TestConfig { get; set; }
        public DbSet<Replication> Replication { get; set; }
        public DbSet<Position> Position { get; set; }
        public DbSet<PosTrade> PosTrade { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<ReposObject> Repository { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<SeriesValue> SeriesValue { get; set; }
        public DbSet<DbChart> Chart { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Instrum
            var instrumBuilder = modelBuilder.Entity<Instrum>().ToTable("instrum");
            instrumBuilder.HasKey(r => r.InsID);
            instrumBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            instrumBuilder.Property(p => p.Ticker).HasColumnName("ticker");
            instrumBuilder.Property(p => p.ShortName).HasColumnName("short_name");
            instrumBuilder.Property(p => p.Name).HasColumnName("name");
            instrumBuilder.Property(p => p.LotSize).HasColumnName("lot_size");
            instrumBuilder.Property(p => p.Decimals).HasColumnName("decimals");
            instrumBuilder.Property(p => p.PriceStep).HasColumnName("price_step");

            // InsStore
            var insStoreBuilder = modelBuilder.Entity<InsStore>().ToTable("insstore");
            insStoreBuilder.HasKey(r => r.InsStoreID);
            insStoreBuilder.Property(p => p.InsStoreID).HasColumnName("insstore_id");
            insStoreBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            insStoreBuilder.Property(p => p.Tf).HasColumnName("tf");
            insStoreBuilder.Property(p => p.IsEnable).HasColumnName("enable");

            // DbBarHistory
            var dbBarHistoryBuilder = modelBuilder.Entity<DbBarHistory>().ToTable("barhistory");
            dbBarHistoryBuilder.HasKey(r => new { r.InsStoreID, r.Time });
            dbBarHistoryBuilder.Property(p => p.InsStoreID).HasColumnName("insstore_id");
            dbBarHistoryBuilder.Property(p => p.Time).HasColumnName("bar_time");
            dbBarHistoryBuilder.Property(p => p.OpenPrice).HasColumnName("open");
            dbBarHistoryBuilder.Property(p => p.CloseDelta).HasColumnName("close_d");
            dbBarHistoryBuilder.Property(p => p.HighDelta).HasColumnName("high_d");
            dbBarHistoryBuilder.Property(p => p.LowDelta).HasColumnName("low_d");
            dbBarHistoryBuilder.Property(p => p.Volume).HasColumnName("volume");

            // DbTickHistory
            var dbTickHistoryBuilder = modelBuilder.Entity<DbTickHistory>().ToTable("tickhistory");
            dbTickHistoryBuilder.HasKey(r => new { r.TickHistoryID });
            dbTickHistoryBuilder.Property(p => p.TickHistoryID).HasColumnName("tickhistory_id");
            dbTickHistoryBuilder.Property(p => p.Date).HasColumnName("hist_date");
            dbTickHistoryBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            dbTickHistoryBuilder.Property(p => p.Data).HasColumnName("data");

            // InsStorePeriods
            var insStorePeriodsBuilder = modelBuilder.Entity<InsStorePeriods>().ToTable("periods");
            insStorePeriodsBuilder.HasKey(r => new { r.InsStoreID, r.StartDate });
            insStorePeriodsBuilder.Property(p => p.InsStoreID).HasColumnName("insstore_id");
            insStorePeriodsBuilder.Property(p => p.StartDate).HasColumnName("start_date");
            insStorePeriodsBuilder.Property(p => p.EndDate).HasColumnName("end_date");
            insStorePeriodsBuilder.Property(p => p.IsLastDirty).HasColumnName("last_dirty");

            // InsStoreFreeDays
            var insStoreFreeDaysBuilder = modelBuilder.Entity<InsStoreFreeDays>().ToTable("freedays");
            insStoreFreeDaysBuilder.HasKey(r => new { r.InsStoreID, r.Date });
            insStoreFreeDaysBuilder.Property(p => p.InsStoreID).HasColumnName("insstore_id");
            insStoreFreeDaysBuilder.Property(p => p.Date).HasColumnName("date");

            // Account
            var accountBuilder = modelBuilder.Entity<Account>().ToTable("account");
            accountBuilder.HasKey(r => r.AccountID);
            accountBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            accountBuilder.Property(p => p.Code).HasColumnName("code");
            accountBuilder.Property(p => p.Name).HasColumnName("name");
            accountBuilder.Property(p => p.CommPerc).HasColumnName("comm_perc");
            accountBuilder.Property(p => p.IsShortEnable).HasColumnName("short_enable");
            accountBuilder.Property(p => p.AccountType).HasColumnName("account_type");

            // Cash
            var cashBuilder = modelBuilder.Entity<Cash>().ToTable("cash");
            cashBuilder.HasKey(r => r.CashID);
            cashBuilder.Property(p => p.CashID).HasColumnName("cash_id");
            cashBuilder.Property(p => p.Initial).HasColumnName("initial");
            cashBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            cashBuilder.Property(p => p.Current).HasColumnName("current");
            cashBuilder.Property(p => p.Sell).HasColumnName("sell");
            cashBuilder.Property(p => p.Buy).HasColumnName("buy");
            cashBuilder.Property(p => p.SellComm).HasColumnName("sell_comm");
            cashBuilder.Property(p => p.BuyComm).HasColumnName("buy_comm");

            // Holding
            var holdingBuilder = modelBuilder.Entity<Holding>().ToTable("holding");
            holdingBuilder.HasKey(r => r.HoldingID);
            holdingBuilder.Property(p => p.HoldingID).HasColumnName("holding_id");
            holdingBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            holdingBuilder.Property(p => p.LotCount).HasColumnName("lots");
            holdingBuilder.Property(p => p.AccountID).HasColumnName("account_id");

            // Order
            var orderBuilder = modelBuilder.Entity<Order>().ToTable("orders");
            orderBuilder.HasKey(r => r.OrderID);
            orderBuilder.Property(p => p.OrderID).HasColumnName("order_id");
            orderBuilder.Property(p => p.Time).HasColumnName("order_time");
            orderBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            orderBuilder.Property(p => p.BuySell).HasColumnName("buy_sell");
            orderBuilder.Property(p => p.LotCount).HasColumnName("lots");
            orderBuilder.Property(p => p.Price).HasColumnName("price");
            orderBuilder.Property(p => p.Status).HasColumnName("status");
            orderBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            orderBuilder.Property(p => p.StopOrderID).HasColumnName("stoporder_id");
            orderBuilder.Property(p => p.OrderNo).HasColumnName("order_no");

            // StopOrder
            var stopOrderBuilder = modelBuilder.Entity<StopOrder>().ToTable("stoporder");
            stopOrderBuilder.HasKey(r => r.StopOrderID);
            stopOrderBuilder.Property(p => p.StopOrderID).HasColumnName("stoporder_id");
            stopOrderBuilder.Property(p => p.Time).HasColumnName("stoporder_time");
            stopOrderBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            stopOrderBuilder.Property(p => p.BuySell).HasColumnName("buy_sell");
            stopOrderBuilder.Property(p => p.StopType).HasColumnName("stop_type");
            stopOrderBuilder.Property(p => p.EndTime).HasColumnName("end_time");
            stopOrderBuilder.Property(p => p.AlertPrice).HasColumnName("alert_price");
            stopOrderBuilder.Property(p => p.Price).HasColumnName("price");
            stopOrderBuilder.Property(p => p.LotCount).HasColumnName("lots");
            stopOrderBuilder.Property(p => p.Status).HasColumnName("status");
            stopOrderBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            stopOrderBuilder.Property(p => p.CompleteTime).HasColumnName("complete_time");
            stopOrderBuilder.Property(p => p.StopOrderNo).HasColumnName("stoporder_no");

            // Trade
            var tradeBuilder = modelBuilder.Entity<Trade>().ToTable("trade");
            tradeBuilder.HasKey(r => r.TradeID);
            tradeBuilder.Property(p => p.TradeID).HasColumnName("trade_id");
            tradeBuilder.Property(p => p.OrderID).HasColumnName("orders_id");
            tradeBuilder.Property(p => p.Time).HasColumnName("trade_time");
            tradeBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            tradeBuilder.Property(p => p.BuySell).HasColumnName("buy_sell");
            tradeBuilder.Property(p => p.LotCount).HasColumnName("lots");
            tradeBuilder.Property(p => p.Price).HasColumnName("price");
            tradeBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            tradeBuilder.Property(p => p.Comm).HasColumnName("comm");
            tradeBuilder.Property(p => p.TradeNo).HasColumnName("trade_no");

            // TickSource
            var tickSourceBuilder = modelBuilder.Entity<DbTickSource>().ToTable("ticksource");
            tickSourceBuilder.HasKey(r => r.TickSourceID);
            tickSourceBuilder.Property(p => p.TickSourceID).HasColumnName("ticksource_id");
            tickSourceBuilder.Property(p => p.Name).HasColumnName("name");
            tickSourceBuilder.Property(p => p.DataStr).HasColumnName("data");

            // TestConfig
            var testConfigBuilder = modelBuilder.Entity<DbTestConfig>().ToTable("testconfig");
            testConfigBuilder.HasKey(r => r.TestConfigID);
            testConfigBuilder.Property(p => p.TestConfigID).HasColumnName("testconfig_id");
            testConfigBuilder.Property(p => p.Name).HasColumnName("name");
            testConfigBuilder.Property(p => p.DataStr).HasColumnName("data");

            // Replication
            var replicationBuilder = modelBuilder.Entity<Replication>().ToTable("replication");
            replicationBuilder.HasKey(r => new { r.ReplObject, r.LocalID, r.RemoteID });
            replicationBuilder.Property(p => p.LocalID).HasColumnName("local_id");
            replicationBuilder.Property(p => p.RemoteID).HasColumnName("remote_id");
            replicationBuilder.Property(p => p.ReplObject).HasColumnName("repl_object");

            // Position
            var positionBuilder = modelBuilder.Entity<Position>().ToTable("positions");
            positionBuilder.HasKey(r => r.PosID);
            positionBuilder.Property(p => p.PosID).HasColumnName("pos_id");
            positionBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            positionBuilder.Property(p => p.Count).HasColumnName("count");
            positionBuilder.Property(p => p.OpenTime).HasColumnName("open_time");
            positionBuilder.Property(p => p.OpenPrice).HasColumnName("open_price");
            positionBuilder.Property(p => p.CloseTime).HasColumnName("close_time");
            positionBuilder.Property(p => p.ClosePrice).HasColumnName("close_price");
            positionBuilder.Property(p => p.PosType).HasColumnName("pos_type");
            positionBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            positionBuilder.Ignore(p => p.IsChanged);

            // PosTrade
            var posTradeBuilder = modelBuilder.Entity<PosTrade>().ToTable("postrade");
            posTradeBuilder.HasKey(r => new { r.PosID, r.TradeID });
            posTradeBuilder.Property(p => p.PosID).HasColumnName("pos_id");
            posTradeBuilder.Property(p => p.TradeID).HasColumnName("trade_id");
            posTradeBuilder.Ignore(p => p.IsNew);

            // User
            var userBuilder = modelBuilder.Entity<User>().ToTable("users");
            userBuilder.HasKey(r => r.UserID);
            userBuilder.Property(p => p.UserID).HasColumnName("user_id");
            userBuilder.Property(p => p.Login).HasColumnName("login");
            userBuilder.Property(p => p.PasswordHash).HasColumnName("pwd_hash");
            userBuilder.Property(p => p.Role).HasColumnName("role");

            // Repository
            var reposBuilder = modelBuilder.Entity<ReposObject>().ToTable("repository");
            reposBuilder.HasKey(r => new { r.ReposID });
            reposBuilder.Property(p => p.ReposID).HasColumnName("repos_id");
            reposBuilder.Property(p => p.Key).HasColumnName("key");
            reposBuilder.Property(p => p.Data).HasColumnName("data");

            // Series
            var seriesBuilder = modelBuilder.Entity<Series>().ToTable("series");
            seriesBuilder.HasKey(r => new { r.SeriesID });
            seriesBuilder.Property(p => p.SeriesID).HasColumnName("series_id");
            seriesBuilder.Property(p => p.Key).HasColumnName("key");
            seriesBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            seriesBuilder.Property(p => p.Name).HasColumnName("name");
            seriesBuilder.Property(p => p.Axis).HasColumnName("axis");
            seriesBuilder.Property(p => p.Data).HasColumnName("data");

            // SeriesValue
            var seriesValueBuilder = modelBuilder.Entity<SeriesValue>().ToTable("seriesvalue");
            seriesValueBuilder.HasKey(r => new { r.SeriesValueID });
            seriesValueBuilder.Property(p => p.SeriesValueID).HasColumnName("seriesvalue_id");
            seriesValueBuilder.Property(p => p.SeriesID).HasColumnName("series_id");
            seriesValueBuilder.Property(p => p.Time).HasColumnName("value_time");
            seriesValueBuilder.Property(p => p.EndTime).HasColumnName("end_time");
            seriesValueBuilder.Property(p => p.Value).HasColumnName("value");
            seriesValueBuilder.Property(p => p.EndValue).HasColumnName("end_value");
            seriesValueBuilder.Property(p => p.Data).HasColumnName("data");

            // Chart
            var chartBuilder = modelBuilder.Entity<DbChart>().ToTable("chart");
            chartBuilder.HasKey(r => r.ChartID);
            chartBuilder.Property(p => p.ChartID).HasColumnName("chart_id");
            chartBuilder.Property(p => p.InsID).HasColumnName("ins_id");
            chartBuilder.Property(p => p.Tf).HasColumnName("tf");
            chartBuilder.Property(p => p.AccountID).HasColumnName("account_id");
            chartBuilder.Property(p => p.Data).HasColumnName("data");
        }
    }
}
