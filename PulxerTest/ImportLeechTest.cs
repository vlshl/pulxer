using Common.Data;
using Common.Interfaces;
using Platform;
using Pulxer;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PulxerTest
{
    public class ImportLeechTest
    {
        [Fact]
        public void SyncAccountDataAsync_accounts_sameAccount()
        {
            IAccountDA accountDA = new AccountDAMock();
            SyncPipeServerMock sps = new SyncPipeServerMock();
            IInstrumDA instrumDA = new InstrumDAMock();
            IReplicationBL replBL = new ReplicationBLMock();

            ImportLeech import = new ImportLeech(instrumDA, accountDA, null, null, replBL, null);
            import.FullSyncAccountDataAsync(sps).Wait();

            var r_accounts = sps.GetAccountList().Result;
            CompareAccounts(r_accounts, accountDA.GetAccounts(), replBL);

            // изменим account
            r_accounts[0].Code = "***";
            r_accounts[0].Name = "+++";
            r_accounts[0].IsShortEnable = !r_accounts[0].IsShortEnable;
            r_accounts[0].CommPerc += 0.5m;

            import.FullSyncAccountDataAsync(sps).Wait();

            CompareAccounts(r_accounts, accountDA.GetAccounts(), replBL);

            // добавим еще account
            sps.AddAccount(Common.Data.AccountTypes.Test, "ccc", "nnn", 0, false);

            import.FullSyncAccountDataAsync(sps).Wait();

            CompareAccounts(sps.GetAccountList().Result, accountDA.GetAccounts(), replBL);

            // удаление account не тестируем
        }

        [Fact]
        public void SyncAccountDataAsync_instrums_sameInstrums()
        {
            IAccountDA accountDA = new AccountDAMock();
            IInstrumDA instrumDA = new InstrumDAMock();
            SyncPipeServerMock sps = new SyncPipeServerMock();
            IReplicationBL replBL = new ReplicationBLMock();

            ImportLeech import = new ImportLeech(instrumDA, accountDA, null, null, replBL, null);
            import.FullSyncAccountDataAsync(sps).Wait();

            var r_instrums = sps.GetInstrumList().Result;
            CompareInstrums(r_instrums, instrumDA.GetInstrums(), replBL);

            // изменим instrum
            r_instrums[0].ShortName = "***";
            r_instrums[0].Name = "+++";
            r_instrums[0].LotSize = 10;
            r_instrums[0].Decimals = 3;

            import.FullSyncAccountDataAsync(sps).Wait();

            CompareInstrums(r_instrums, instrumDA.GetInstrums(), replBL);

            // новый инструмент
            var newIns = sps.AddInstrum("ttt", "sn");

            import.FullSyncAccountDataAsync(sps).Wait();

            CompareInstrums(sps.GetInstrumList().Result, instrumDA.GetInstrums(), replBL);

            // удалим инструмент
            sps.RemoveInstrum(newIns.InsID);

            import.FullSyncAccountDataAsync(sps).Wait();

            // при удалении инструмента в удаленной базе,
            // в локальной базе мы его не удаляем, 
            // а удаляем лишь репликацию
            bool isReplFound = replBL.GetReplications(Common.Data.ReplObjects.Instrum).ContainsKey(newIns.InsID);
            Assert.False(isReplFound);

            // в удаленной базе должно быть на 1 меньше
            Assert.Equal(sps.GetInstrumList().Result.Count(), instrumDA.GetInstrums().Count() - 1);
        }

        [Fact]
        public void SyncAccountDataAsync_data_sameData()
        {
            IInstrumDA instrumDA = new InstrumDAMock();
            IAccountDA accountDA = new AccountDAMock();
            SyncPipeServerMock sps = new SyncPipeServerMock();
            IReplicationBL replBL = new ReplicationBLMock();

            var r_accounts = sps.GetAccountList().Result;
            var r_instrums = sps.GetInstrumList().Result;

            var so1 = sps.AddStopOrder(r_accounts[0].AccountID, r_instrums[0].InsID, Platform.BuySell.Buy, Platform.StopOrderType.StopLoss, 1000, 1);
            var ord1 = sps.AddOrder(so1);
            var trd1 = sps.AddTrade(ord1);
            var ord2 = sps.AddOrder(r_accounts[1].AccountID, r_instrums[1].InsID, Platform.BuySell.Sell, 1000, 1);
            var trd2 = sps.AddTrade(ord2);

            // действительно добавили записи
            Assert.True(sps.GetStopOrders(r_accounts[0].AccountID, 0).Result.Count() == 1);
            Assert.True(sps.GetOrders(r_accounts[0].AccountID, 0).Result.Count() == 1);
            Assert.True(sps.GetTrades(r_accounts[0].AccountID, 0).Result.Count() == 1);
            Assert.True(sps.GetStopOrders(r_accounts[1].AccountID, 0).Result.Count() == 0);
            Assert.True(sps.GetOrders(r_accounts[1].AccountID, 0).Result.Count() == 1);
            Assert.True(sps.GetTrades(r_accounts[1].AccountID, 0).Result.Count() == 1);

            ImportLeech import = new ImportLeech(instrumDA, accountDA, null, null, replBL, null);
            import.FullSyncAccountDataAsync(sps).Wait();

            // узнаем локальные accountID 
            var repl_acc = replBL.GetReplications(Common.Data.ReplObjects.Account);
            int l_acc0ID = repl_acc[r_accounts[0].AccountID];
            int l_acc1ID = repl_acc[r_accounts[1].AccountID];

            CompareStopOrders(sps.GetStopOrders(r_accounts[0].AccountID, 0).Result, accountDA.GetStopOrders(l_acc0ID), replBL);
            CompareStopOrders(sps.GetStopOrders(r_accounts[1].AccountID, 0).Result, accountDA.GetStopOrders(l_acc1ID), replBL);
            CompareOrders(sps.GetOrders(r_accounts[0].AccountID, 0).Result, accountDA.GetOrders(l_acc0ID), replBL);
            CompareOrders(sps.GetOrders(r_accounts[1].AccountID, 0).Result, accountDA.GetOrders(l_acc1ID), replBL);
            CompareTrades(sps.GetTrades(r_accounts[0].AccountID, 0).Result, accountDA.GetTrades(l_acc0ID), replBL);
            CompareTrades(sps.GetTrades(r_accounts[1].AccountID, 0).Result, accountDA.GetTrades(l_acc1ID), replBL);

            // еще добавили данных
            var so3 = sps.AddStopOrder(r_accounts[0].AccountID, r_instrums[0].InsID, Platform.BuySell.Sell, Platform.StopOrderType.TakeProfit, 2000, 2);
            var ord3 = sps.AddOrder(so3);
            var trd3 = sps.AddTrade(ord3);
            var ord4 = sps.AddOrder(r_accounts[1].AccountID, r_instrums[1].InsID, Platform.BuySell.Buy, 3000, 3);
            var trd4 = sps.AddTrade(ord4);

            // снова синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // и снова сравнили
            CompareStopOrders(sps.GetStopOrders(r_accounts[0].AccountID, 0).Result, accountDA.GetStopOrders(l_acc0ID), replBL);
            CompareStopOrders(sps.GetStopOrders(r_accounts[1].AccountID, 0).Result, accountDA.GetStopOrders(l_acc1ID), replBL);
            CompareOrders(sps.GetOrders(r_accounts[0].AccountID, 0).Result, accountDA.GetOrders(l_acc0ID), replBL);
            CompareOrders(sps.GetOrders(r_accounts[1].AccountID, 0).Result, accountDA.GetOrders(l_acc1ID), replBL);
            CompareTrades(sps.GetTrades(r_accounts[0].AccountID, 0).Result, accountDA.GetTrades(l_acc0ID), replBL);
            CompareTrades(sps.GetTrades(r_accounts[1].AccountID, 0).Result, accountDA.GetTrades(l_acc1ID), replBL);

            // изменили записи
            so3.Status = StopOrderStatus.Reject;
            ord3.Status = OrderStatus.Reject;

            // снова синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // и снова сравнили
            CompareStopOrders(sps.GetStopOrders(r_accounts[0].AccountID, 0).Result, accountDA.GetStopOrders(l_acc0ID), replBL);
            CompareStopOrders(sps.GetStopOrders(r_accounts[1].AccountID, 0).Result, accountDA.GetStopOrders(l_acc1ID), replBL);
            CompareOrders(sps.GetOrders(r_accounts[0].AccountID, 0).Result, accountDA.GetOrders(l_acc0ID), replBL);
            CompareOrders(sps.GetOrders(r_accounts[1].AccountID, 0).Result, accountDA.GetOrders(l_acc1ID), replBL);
            CompareTrades(sps.GetTrades(r_accounts[0].AccountID, 0).Result, accountDA.GetTrades(l_acc0ID), replBL);
            CompareTrades(sps.GetTrades(r_accounts[1].AccountID, 0).Result, accountDA.GetTrades(l_acc1ID), replBL);
        }

        [Fact]
        public void SyncAccountDataAsync_holdings_sameHoldings()
        {
            IInstrumDA instrumDA = new InstrumDAMock();
            IAccountDA accountDA = new AccountDAMock();
            SyncPipeServerMock sps = new SyncPipeServerMock();
            IReplicationBL replBL = new ReplicationBLMock();
            ImportLeech import = new ImportLeech(instrumDA, accountDA, null, null, replBL, null);

            var r_accounts = sps.GetAccountList().Result;
            var r_instrums = sps.GetInstrumList().Result;

            // добавили две записи в разные accounts
            var r_h1 = sps.AddHolding(r_accounts[0].AccountID, r_instrums[0].InsID, 100);
            var r_h2 = sps.AddHolding(r_accounts[1].AccountID, r_instrums[1].InsID, 200);

            // синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // узнаем локальные accountID 
            var repl_acc = replBL.GetReplications(Common.Data.ReplObjects.Account);
            int l_acc0ID = repl_acc[r_accounts[0].AccountID];
            int l_acc1ID = repl_acc[r_accounts[1].AccountID];

            // сравниваем
            CompareHoldings(sps.GetHoldingList(r_accounts[0].AccountID).Result, accountDA.GetHoldings(l_acc0ID), replBL);
            CompareHoldings(sps.GetHoldingList(r_accounts[1].AccountID).Result, accountDA.GetHoldings(l_acc1ID), replBL);

            // поменяем записи
            r_h1.LotCount = 1000;
            r_h2.LotCount = 2000;

            // синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // сравниваем
            CompareHoldings(sps.GetHoldingList(r_accounts[0].AccountID).Result, accountDA.GetHoldings(l_acc0ID), replBL);
            CompareHoldings(sps.GetHoldingList(r_accounts[1].AccountID).Result, accountDA.GetHoldings(l_acc1ID), replBL);

            // еще добавили запись
            var r_h3 = sps.AddHolding(r_accounts[1].AccountID, r_instrums[2].InsID, 300);

            // синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // сравниваем
            CompareHoldings(sps.GetHoldingList(r_accounts[0].AccountID).Result, accountDA.GetHoldings(l_acc0ID), replBL);
            CompareHoldings(sps.GetHoldingList(r_accounts[1].AccountID).Result, accountDA.GetHoldings(l_acc1ID), replBL);

            // удалили записи
            sps.RemoveHolding(r_h1);
            sps.RemoveHolding(r_h2);

            // синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // сравниваем
            CompareHoldings(sps.GetHoldingList(r_accounts[0].AccountID).Result, accountDA.GetHoldings(l_acc0ID), replBL);
            CompareHoldings(sps.GetHoldingList(r_accounts[1].AccountID).Result, accountDA.GetHoldings(l_acc1ID), replBL);
        }

        [Fact]
        public void SyncAccountDataAsync_cash_sameCash()
        {
            IInstrumDA instrumDA = new InstrumDAMock();
            IAccountDA accountDA = new AccountDAMock();
            SyncPipeServerMock sps = new SyncPipeServerMock();
            IReplicationBL replBL = new ReplicationBLMock();
            ImportLeech import = new ImportLeech(instrumDA, accountDA, null, null, replBL, null);

            var r_accounts = sps.GetAccountList().Result;
            var r_instrums = sps.GetInstrumList().Result;

            // добавили две записи в разные accounts
            var r_h1 = sps.AddCash(r_accounts[0].AccountID, 100);
            var r_h2 = sps.AddCash(r_accounts[1].AccountID, 200);

            // синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // узнаем локальные accountID 
            var repl_acc = replBL.GetReplications(Common.Data.ReplObjects.Account);
            int l_acc0ID = repl_acc[r_accounts[0].AccountID];
            int l_acc1ID = repl_acc[r_accounts[1].AccountID];

            // сравниваем
            var rCash = sps.GetCash(r_accounts[0].AccountID).Result;
            var lCash = accountDA.GetCash(l_acc0ID);
            CompareCash(rCash, lCash);

            // изменили данные
            rCash.Initial += 10.0m;
            rCash.Current += 20.0m;
            rCash.Buy += 30.0m;
            rCash.BuyComm += 40.0m;
            rCash.Sell += 50.0m;
            rCash.SellComm += 60.0m;

            // синхронизировали
            import.FullSyncAccountDataAsync(sps).Wait();

            // сравниваем
            var rCash1 = sps.GetCash(r_accounts[0].AccountID).Result;
            var lCash1 = accountDA.GetCash(l_acc0ID);
            CompareCash(rCash1, lCash1);
        }

        private void CompareAccounts(IEnumerable<Common.Data.Account> r_accounts, IEnumerable<Common.Data.Account> l_accounts,
            IReplicationBL replBL)
        {
            Assert.True(l_accounts.Count() == r_accounts.Count());
            var repl_acc = replBL.GetReplications(Common.Data.ReplObjects.Account);
            foreach (var r_acc in r_accounts)
            {
                Assert.True(repl_acc.ContainsKey(r_acc.AccountID));
                int l_accID = repl_acc[r_acc.AccountID];
                var l_acc = l_accounts.FirstOrDefault(r => r.AccountID == l_accID);
                Assert.NotNull(l_acc);
                Assert.True(l_acc.Code == r_acc.Code);
                Assert.True(l_acc.Name == r_acc.Name);
                Assert.True(l_acc.AccountType == r_acc.AccountType);
                Assert.True(l_acc.CommPerc == r_acc.CommPerc);
                Assert.True(l_acc.IsShortEnable == r_acc.IsShortEnable);
            }
        }

        private void CompareInstrums(IEnumerable<Common.Data.Instrum> r_instrums, IEnumerable<Common.Data.Instrum> l_instrums,
            IReplicationBL replBL)
        {
            Assert.True(l_instrums.Count() == r_instrums.Count());
            var repl_ins = replBL.GetReplications(Common.Data.ReplObjects.Instrum);
            foreach (var r_ins in r_instrums)
            {
                Assert.True(repl_ins.ContainsKey(r_ins.InsID));
                int l_insID = repl_ins[r_ins.InsID];
                var l_ins = l_instrums.FirstOrDefault(r => r.InsID == l_insID);
                Assert.NotNull(l_ins);
                Assert.True(l_ins.Ticker == r_ins.Ticker);
                Assert.True(l_ins.ShortName == r_ins.ShortName);
                Assert.True(l_ins.Name == r_ins.Name);
                Assert.True(l_ins.Decimals == r_ins.Decimals);
                Assert.True(l_ins.LotSize == r_ins.LotSize);
                Assert.True(l_ins.PriceStep == r_ins.PriceStep);
            }
        }

        private void CompareOrders(IEnumerable<Order> r_orders, IEnumerable<Order> l_orders, IReplicationBL replBL)
        {
            Assert.True(l_orders.Count() == r_orders.Count());
            var repl = replBL.GetReplications(Common.Data.ReplObjects.Order);
            var repl_acc = replBL.GetReplications(Common.Data.ReplObjects.Account);
            var repl_ins = replBL.GetReplications(Common.Data.ReplObjects.Instrum);
            var repl_so = replBL.GetReplications(Common.Data.ReplObjects.StopOrder);

            foreach (var r_ord in r_orders)
            {
                Assert.True(repl.ContainsKey(r_ord.OrderID));
                int l_ordID = repl[r_ord.OrderID];
                var l_ord = l_orders.FirstOrDefault(r => r.OrderID == l_ordID);

                Assert.NotNull(l_ord);

                Assert.True(l_ord.AccountID == repl_acc[r_ord.AccountID]);
                Assert.True(l_ord.BuySell == r_ord.BuySell);
                Assert.True(l_ord.InsID == repl_ins[r_ord.InsID]);
                Assert.True(l_ord.LotCount == r_ord.LotCount);
                Assert.True(l_ord.OrderID == l_ordID);
                Assert.True(l_ord.OrderNo == r_ord.OrderNo);
                Assert.True(l_ord.Price == r_ord.Price);
                Assert.True(l_ord.Status == r_ord.Status);

                int? l_stopOrderID = null;
                if (r_ord.StopOrderID != null) l_stopOrderID = repl_so[r_ord.StopOrderID.Value];
                Assert.True(l_ord.StopOrderID == l_stopOrderID);

                Assert.True(l_ord.Time == r_ord.Time);
            }
        }

        private void CompareStopOrders(IEnumerable<StopOrder> r_stopOrders, IEnumerable<StopOrder> l_stopOrders, IReplicationBL replBL)
        {
            Assert.True(l_stopOrders.Count() == r_stopOrders.Count());
            var repl = replBL.GetReplications(Common.Data.ReplObjects.StopOrder);
            var repl_acc = replBL.GetReplications(Common.Data.ReplObjects.Account);
            var repl_ins = replBL.GetReplications(Common.Data.ReplObjects.Instrum);

            foreach (var r_so in r_stopOrders)
            {
                Assert.True(repl.ContainsKey(r_so.StopOrderID));
                int l_soID = repl[r_so.StopOrderID];
                var l_so = l_stopOrders.FirstOrDefault(r => r.StopOrderID == l_soID);

                Assert.NotNull(l_so);

                Assert.True(l_so.AccountID == repl_acc[r_so.AccountID]);
                Assert.True(l_so.AlertPrice == r_so.AlertPrice);
                Assert.True(l_so.BuySell == r_so.BuySell);
                Assert.True(l_so.CompleteTime == r_so.CompleteTime);
                Assert.True(l_so.EndTime == r_so.EndTime);
                Assert.True(l_so.InsID == repl_ins[r_so.InsID]);
                Assert.True(l_so.LotCount == r_so.LotCount);
                Assert.True(l_so.Price == r_so.Price);
                Assert.True(l_so.Status == r_so.Status);
                Assert.True(l_so.StopOrderID == l_soID);
                Assert.True(l_so.StopOrderNo == r_so.StopOrderNo);
                Assert.True(l_so.StopType == r_so.StopType);
                Assert.True(l_so.Time == r_so.Time);
            }
        }

        private void CompareTrades(IEnumerable<Trade> r_trades, IEnumerable<Trade> l_trades, IReplicationBL replBL)
        {
            Assert.True(l_trades.Count() == r_trades.Count());
            var repl = replBL.GetReplications(Common.Data.ReplObjects.Trade);
            var repl_acc = replBL.GetReplications(Common.Data.ReplObjects.Account);
            var repl_ins = replBL.GetReplications(Common.Data.ReplObjects.Instrum);
            var repl_ord = replBL.GetReplications(Common.Data.ReplObjects.Order);

            foreach (var r_trd in r_trades)
            {
                Assert.True(repl.ContainsKey(r_trd.TradeID));
                int l_trdID = repl[r_trd.TradeID];
                var l_trd = l_trades.FirstOrDefault(r => r.TradeID == l_trdID);

                Assert.NotNull(l_trd);

                Assert.True(l_trd.AccountID == repl_acc[r_trd.AccountID]);
                Assert.True(l_trd.BuySell == r_trd.BuySell);
                Assert.True(l_trd.Comm == r_trd.Comm);
                Assert.True(l_trd.InsID == repl_ins[r_trd.InsID]);
                Assert.True(l_trd.LotCount == r_trd.LotCount);

                int l_orderID = repl_ord[r_trd.OrderID];
                Assert.True(l_trd.OrderID == l_orderID);

                Assert.True(l_trd.Price == r_trd.Price);
                Assert.True(l_trd.Time == r_trd.Time);
                Assert.True(l_trd.TradeID == l_trdID);
                Assert.True(l_trd.TradeNo == r_trd.TradeNo);
            }
        }

        private void CompareHoldings(IEnumerable<Holding> r_holdings, IEnumerable<Holding> l_holdings, IReplicationBL replBL)
        {
            Assert.True(r_holdings.Count() == l_holdings.Count());
            var repl_ins = replBL.GetReplications(ReplObjects.Instrum);
            var repl_acc = replBL.GetReplications(ReplObjects.Account);

            // сначала просматриваем все remote-записи
            // для каждой должна найтись одна локальная с тем же инструментом и кол-вом
            foreach (var r_hold in r_holdings)
            {
                int l_accID = repl_acc[r_hold.AccountID];
                int l_insID = repl_ins[r_hold.InsID];
                var l_holds = l_holdings.Where(r => r.AccountID == l_accID && r.InsID == l_insID);
                Assert.True(l_holds.Count() == 1);
                var l_hold = l_holds.ElementAt(0);

                Assert.True(l_hold.LotCount == r_hold.LotCount);
            }

            // теперь просматриваем все ловальные
            // на каждую должна найтись одна remote-запись с тем же инструментом и кол-вом
            foreach (var l_hold in l_holdings)
            {
                int r_accID = repl_acc.FirstOrDefault(r => r.Value == l_hold.AccountID).Key;
                int r_insID = repl_ins.FirstOrDefault(r => r.Value == l_hold.InsID).Key;

                var r_holds = r_holdings.Where(r => r.AccountID == r_accID && r.InsID == r_insID);
                Assert.True(r_holds.Count() == 1);
                var r_hold = r_holds.ElementAt(0);

                Assert.True(r_hold.LotCount == l_hold.LotCount);
            }
        }

        private void CompareCash(Cash rCash, Cash lCash)
        {
            Assert.NotNull(rCash);
            Assert.NotNull(lCash);
            Assert.True(lCash.Current == rCash.Current && lCash.Initial == rCash.Initial &&
                lCash.Sell == rCash.Sell && lCash.SellComm == rCash.SellComm &&
                lCash.Buy == rCash.Buy && lCash.BuyComm == rCash.BuyComm);
        }
    }
}
