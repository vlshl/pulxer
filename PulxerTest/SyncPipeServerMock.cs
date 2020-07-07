using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PulxerTest
{
    public class SyncPipeServerMock : ISyncPipeServer
    {
        private List<Account> _accounts;
        private List<Instrum> _instrums;
        private List<StopOrder> _stopOrders;
        private List<Order> _orders;
        private List<Trade> _trades;
        private List<Cash> _cashes;
        private List<Holding> _holdings;
        private int _cashID = 1;
        private int _holdingID = 1;

        public SyncPipeServerMock()
        {
            _accounts = new List<Account>()
            {
                new Account() { AccountID = 1, AccountType = AccountTypes.Real, Code = "acc1", CommPerc = 0.01m, IsShortEnable = false, Name = "Account 1" },
                new Account() { AccountID = 2, AccountType = AccountTypes.Test, Code = "acc2", CommPerc = 0.05m, IsShortEnable = true, Name = "Account 2" }
            };

            _instrums = new List<Instrum>()
            {
                new Instrum() { InsID = 1, Ticker = "ticker1", Name = "instrum 1", ShortName = "ins1", Decimals = 0, LotSize = 1, PriceStep = 0.01m },
                new Instrum() { InsID = 2, Ticker = "ticker2", Name = "instrum 2", ShortName = "ins2", Decimals = 0, LotSize = 1, PriceStep = 0.01m },
                new Instrum() { InsID = 3, Ticker = "ticker3", Name = "instrum 3", ShortName = "ins3", Decimals = 0, LotSize = 1, PriceStep = 0.01m }
            };

            _stopOrders = new List<StopOrder>();
            _orders = new List<Order>();
            _trades = new List<Trade>();
            _cashes = new List<Cash>();
            _holdings = new List<Holding>();
        }

        #region Account
        public Task<Account[]> GetAccountList()
        {
            return Task.Run(() => { return _accounts.ToArray(); });
        }

        public Account AddAccount(AccountTypes at, string code, string name, decimal commperc, bool isShortEnable)
        {
            var acc = new Account()
            {
                AccountID = _accounts.Count + 1,
                AccountType = AccountTypes.Real,
                Code = code,
                CommPerc = commperc,
                IsShortEnable = isShortEnable,
                Name = name
            };
            _accounts.Add(acc);

            return acc;
        }
        #endregion

        #region Instrum
        public Task<Instrum[]> GetInstrumList()
        {
            return Task.Run(() => { return _instrums.ToArray(); });
        }

        public Instrum AddInstrum(string ticker, string shortName)
        {
            var ins = new Instrum()
            {
                InsID = _instrums.Count + 1,
                Ticker = ticker,
                ShortName = shortName,
                Name = shortName,
                Decimals = 1,
                PriceStep = 0.01m,
                LotSize = 1
            };
            _instrums.Add(ins);

            return ins;
        }

        public void RemoveInstrum(int insID)
        {
            var ins = _instrums.FirstOrDefault(r => r.InsID == insID);
            if (ins != null) _instrums.Remove(ins);
        }
        #endregion

        #region StopOrder
        public Task<StopOrder[]> GetStopOrderList(int accountID)
        {
            return Task.Run(() => { return _stopOrders.Where(r => r.AccountID == accountID).ToArray(); });
        }

        public StopOrder AddStopOrder(int accountID, int insID, BuySell bs, StopOrderType st, decimal price, int lots)
        {
            StopOrder so = new StopOrder()
            {
                StopOrderID = _stopOrders.Count + 1,
                AccountID = accountID,
                InsID = insID,
                BuySell = bs,
                StopType = st,
                Time = DateTime.Now,
                CompleteTime = null,
                EndTime = null,
                Price = price,
                AlertPrice = price,
                LotCount = lots,
                StopOrderNo = 1000 * (_stopOrders.Count + 1),
                Status = StopOrderStatus.Active
            };
            _stopOrders.Add(so);

            return so;
        }
        #endregion

        #region Order
        public Task<Order[]> GetOrderList(int accountID)
        {
            return Task.Run(() => { return _orders.Where(r => r.AccountID == accountID).ToArray(); });
        }

        public Order AddOrder(int accountID, int insID, BuySell bs, decimal price, int lots)
        {
            Order ord = new Order()
            {
                OrderID = _orders.Count + 1,
                AccountID = accountID,
                StopOrderID = null,
                InsID = insID,
                BuySell = bs,
                Time = DateTime.Now,
                Price = price,
                LotCount = lots,
                OrderNo = 1000 * (_orders.Count + 1),
                Status = OrderStatus.Active
            };
            _orders.Add(ord);

            return ord;
        }

        public Order AddOrder(StopOrder so)
        {
            Order ord = new Order()
            {
                OrderID = _orders.Count + 1,
                AccountID = so.AccountID,
                StopOrderID = so.StopOrderID,
                InsID = so.InsID,
                BuySell = so.BuySell,
                Time = DateTime.Now,
                Price = so.Price,
                LotCount = so.LotCount,
                OrderNo = 1000 * (_orders.Count + 1),
                Status = OrderStatus.Active
            };
            _orders.Add(ord);

            return ord;
        }
        #endregion

        #region Trade
        public Task<Trade[]> GetTradeList(int accountID)
        {
            return Task.Run(() => { return _trades.Where(r => r.AccountID == accountID).ToArray(); });
        }

        public Trade AddTrade(Order ord)
        {
            Trade trd = new Trade()
            {
                TradeID = _trades.Count + 1,
                OrderID = ord.OrderID,
                Time = DateTime.Now,
                InsID = ord.InsID,
                BuySell = ord.BuySell,
                LotCount = ord.LotCount,
                Price = ord.Price.Value,
                AccountID = ord.AccountID,
                Comm = 0,
                TradeNo = 1000 * (_trades.Count + 1)
            };
            _trades.Add(trd);

            return trd;
        }
        #endregion

        #region Holding
        public Task<Holding[]> GetHoldingList(int accountID)
        {
            return Task.Run(() => { return _holdings.Where(r => r.AccountID == accountID).ToArray(); });
        }

        public Holding AddHolding(int accountID, int insID, int lotCount)
        {
            Holding holding = new Holding()
            {
                HoldingID = _holdingID++,
                AccountID = accountID,
                InsID = insID,
                LotCount = lotCount
            };
            _holdings.Add(holding);

            return holding;
        }

        public void RemoveHolding(Holding holding)
        {
            _holdings.Remove(holding);
        }
        #endregion

        #region Cash
        public Task<Cash> GetCash(int accountID)
        {
            return Task.Run(() => { return _cashes.FirstOrDefault(r => r.AccountID == accountID); });
        }

        public Cash AddCash(int accountID, decimal init)
        {
            Cash cash = new Cash()
            {
                CashID = _cashID++,
                AccountID = accountID,
                Initial = init,
                Current = init,
                Sell = 0,
                Buy = 0,
                SellComm = 0,
                BuyComm = 0
            };
            _cashes.Add(cash);

            return cash;
        }

        public void RemoveCash(Cash cash)
        {
            _cashes.Remove(cash);
        }
        #endregion
    }
}
