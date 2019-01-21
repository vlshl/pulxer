using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Platform;
using Storage.DbModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage
{
    public class LeechDAContext : DbContext
    {
        public LeechDAContext(DbContextOptions<LeechDAContext> options) : base(options)
        {
        }

        public DbSet<Instrum> Instrum { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<LeechStopOrder> StopOrder { get; set; }
        public DbSet<LeechOrder> Order { get; set; }
        public DbSet<LeechTrade> Trade { get; set; }
        public DbSet<Cash> Cash { get; set; }
        public DbSet<Holding> Holding { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Instrum>().HasKey(r => r.InsID);
            modelBuilder.Entity<Account>().HasKey(r => r.AccountID);
            modelBuilder.Entity<LeechStopOrder>().HasKey(r => r.StopOrderID);
            modelBuilder.Entity<LeechOrder>().HasKey(r => r.OrderID);
            modelBuilder.Entity<LeechTrade>().HasKey(r => r.TradeID);
            modelBuilder.Entity<Cash>().HasKey(r => r.CashID);
            modelBuilder.Entity<Holding>().HasKey(r => r.HoldingID);
        }
    }
}
