using Common.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Storage.Test
{
    public class AccountTest
    {
        private readonly DbContextOptions<DaContext> _options;

        public AccountTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;
        }

        [Fact]
        public void CreateGet_()
        {
            var accDA = new AccountDA(_options);
            var acc = accDA.CreateAccount("code", "name", 100, true, AccountTypes.Test);
            var accounts = accDA.GetAccounts().ToList();

            var accs = accounts.Where(a => a.AccountID == acc.AccountID).ToList();
            Assert.Single(accs);
            var acc1 = accs[0];

            Assert.Equal(acc.Code, acc1.Code);
            Assert.Equal(acc.Name, acc1.Name);
            Assert.Equal(acc.CommPerc, acc1.CommPerc);
            Assert.Equal(acc.IsShortEnable, acc1.IsShortEnable);
            Assert.Equal(acc.AccountType, acc1.AccountType);

            // cleanup
            accDA.DeleteAccount(acc.AccountID);
        }

        [Fact]
        public void UpdateDelete_()
        {
            int accID;

            // create
            var accDA = new AccountDA(_options);
            accID = accDA.CreateAccount("code", "name", 100, true, AccountTypes.Test).AccountID;

            var acc = accDA.GetAccounts().FirstOrDefault(r => r.AccountID == accID);

            acc.Code = "code1";
            acc.Name = "name1";
            acc.CommPerc = 200;
            acc.IsShortEnable = false;
            acc.AccountType = AccountTypes.Real;

            accDA.UpdateAccount(acc);

            var acc1 = accDA.GetAccounts().FirstOrDefault(r => r.AccountID == accID);

            Assert.Equal(accID, acc1.AccountID);
            Assert.Equal("code1", acc1.Code);
            Assert.Equal("name1", acc1.Name);
            Assert.Equal(200, acc1.CommPerc);
            Assert.False(acc1.IsShortEnable);
            Assert.Equal(AccountTypes.Real, acc1.AccountType);

            // delete and cleanp
            accDA.DeleteAccount(accID);

            var list = accDA.GetAccounts();

            Assert.Empty(list.Where(a => a.AccountID == accID));
        }

        [Fact]
        public void DeleteAccountData_()
        {
            int accID;

            // create
            var accDA = new AccountDA(_options);
            var insDA = new InstrumDA(_options);
            var posDA = new PositionDA(_options);

            int insID = insDA.InsertInstrum("", "", "", 1, 0, 1);
            accID = accDA.CreateAccount("code", "name", 100, true, AccountTypes.Test).AccountID;
            accDA.CreateCash(accID, 0, 0, 0, 0, 0, 0);
            var order = accDA.CreateOrder(accID, DateTime.Now, insID, Platform.BuySell.Buy, 1, null, Platform.OrderStatus.Active, null, 0);
            var trade = accDA.CreateTrade(accID, order.OrderID, DateTime.Now, insID, Platform.BuySell.Buy, 1, 0, 0, 0);
            var so = accDA.CreateStopOrder(accID, DateTime.Now, insID, Platform.BuySell.Buy, Platform.StopOrderType.StopLoss, null, 0, null, 0, Platform.StopOrderStatus.Active, null, 0);
            var h = accDA.CreateHolding(accID, insID, 1);
            var pos = posDA.CreatePosition(accID, insID, PosTypes.Long, DateTime.Now, 0, 0, null, null);
            posDA.AddPosTrade(pos.PosID, trade.TradeID);

            accDA.DeleteAccountData(accID);

            Assert.Null(accDA.GetCash(accID)); // данные удалились
            Assert.Empty(accDA.GetOrders(accID));
            Assert.Empty(accDA.GetStopOrders(accID));
            Assert.Empty(accDA.GetTrades(accID));
            Assert.Empty(accDA.GetHoldings(accID));
            Assert.Empty(posDA.GetPosTrades(new List<int>() { pos.PosID }));
            Assert.Empty(posDA.GetPositions(accID, false));
            Assert.NotNull(accDA.GetAccountByID(accID)); // а сам account остался

            // cleanup
            insDA.DeleteInstrumByID(insID);
            accDA.DeleteAccount(accID);
        }

        [Fact]
        public void Series_CreateGetDelete()
        {
            int accID;

            // create account
            var accDA = new AccountDA(_options);
            accID = accDA.CreateAccount("code", "name", 100, true, AccountTypes.Test).AccountID;

            var s1 = accDA.CreateSeries(accID, "key1", "name1", Platform.SeriesAxis.LeftAxis, "data1");

            var series = accDA.GetSeries(accID).ToList();
            Assert.Single(series);
            Assert.Equal(accID, series[0].AccountID);
            Assert.Equal("key1", series[0].Key);
            Assert.Equal("name1", series[0].Name);
            Assert.Equal(Platform.SeriesAxis.LeftAxis, series[0].Axis);
            Assert.Equal("data1", series[0].Data);

            var s2 = accDA.CreateSeries(accID, "key2", "name2", Platform.SeriesAxis.AxisX, "data2");
            var series2 = accDA.GetSeries(accID).ToList();
            Assert.Equal(2, series2.Count);

            // create series values

            accDA.CreateValues(new List<Platform.SeriesValue>()
            {
                new Platform.SeriesValue() { SeriesID = s1.SeriesID, Time = new DateTime(2010, 1, 1, 10, 0, 0), EndTime = null, Value = 100, EndValue = null, Data = "d1" }
            });
            accDA.CreateValues(new List<Platform.SeriesValue>()
            {
                new Platform.SeriesValue() { SeriesID = s2.SeriesID, Time = new DateTime(2010, 1, 1, 10, 0, 0), EndTime = new DateTime(2010, 1, 1, 10, 0, 1), Value = 1000, EndValue = 2000, Data = "d2" }
            });

            var vals1 = accDA.GetValues(s1.SeriesID).ToList();
            var vals2 = accDA.GetValues(s2.SeriesID).ToList();

            Assert.Single(vals1);
            Assert.Single(vals2);
            Assert.Equal(s1.SeriesID, vals1[0].SeriesID);
            Assert.Equal(new DateTime(2010, 1, 1, 10, 0, 0), vals1[0].Time);
            Assert.Null(vals1[0].EndTime);
            Assert.Equal(100, vals1[0].Value);
            Assert.Null(vals1[0].EndValue);
            Assert.Equal("d1", vals1[0].Data);

            // delete and cleanp
            accDA.DeleteAccountData(accID);
            accDA.DeleteAccount(accID);
        }
    }
}
