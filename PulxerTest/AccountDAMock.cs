using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulxerTest
{
    public class AccountDAMock : IAccountDA
    {
        private Dictionary<int, Account> _id_accounts = new Dictionary<int, Account>();
        private Dictionary<int, Order> _id_orders = new Dictionary<int, Order>();
        private Dictionary<int, StopOrder> _id_stopOrders = new Dictionary<int, StopOrder>();
        private Dictionary<int, Trade> _id_trades = new Dictionary<int, Trade>();
        private Dictionary<int, Holding> _id_holdings = new Dictionary<int, Holding>();
        private Dictionary<int, Cash> _id_cash = new Dictionary<int, Cash>();
        private int _nextAccountID = 1001;
        private int _nextOrderID = 1001;
        private int _nextStopOrderID = 1001;
        private int _nextTradeID = 1001;
        private int _nextHoldingID = 1001;
        private int _nextCashID = 1001;

        #region Account
        public IEnumerable<Account> GetAccounts()
        {
            return _id_accounts.Values.ToList();
        }

        public Account GetAccountByID(int accountID)
        {
            throw new NotImplementedException();
        }

        public Account CreateAccount(string code, string name, decimal commPerc, bool isShortEnable, AccountTypes accType)
        {
            Account acc = new Account()
            {
                AccountID = _nextAccountID++,
                Code = code,
                Name = name,
                CommPerc = commPerc,
                IsShortEnable = isShortEnable,
                AccountType = accType
            };
            _id_accounts.Add(acc.AccountID, acc);

            return acc;
        }

        public void UpdateAccount(Account account)
        {
            if (!_id_accounts.ContainsKey(account.AccountID)) return;
            _id_accounts[account.AccountID] = account;
        }

        public void DeleteAccountData(int accountID)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region StopOrder
        public IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID = null)
        {
            var list = _id_stopOrders.Values.Where(r => r.AccountID == accountID);
            if (fromID != null) list = list.Where(r => r.StopOrderID >= fromID.Value);
            return list.ToList();
        }

        public IEnumerable<StopOrder> GetStopOrders(IEnumerable<int> ids)
        {
            return _id_stopOrders.Values.Where(r => ids.Contains(r.StopOrderID)).ToList();
        }

        public StopOrder CreateStopOrder(int accountID, DateTime time, int insID, BuySell bs, StopOrderType st, DateTime? endTime, decimal alertPrice, decimal? price, int lotCount,
    StopOrderStatus status, DateTime? completeTime, long stopOrderNo)
        {
            StopOrder so = new StopOrder()
            {
                StopOrderID = _nextStopOrderID++,
                AccountID = accountID,
                Time = time,
                InsID = insID,
                BuySell = bs,
                StopType = st,
                EndTime = endTime,
                AlertPrice = alertPrice,
                Price = price,
                LotCount = lotCount,
                Status = status,
                CompleteTime = completeTime,
                StopOrderNo = stopOrderNo
            };
            _id_stopOrders.Add(so.StopOrderID, so);

            return so;
        }

        public void UpdateStopOrder(StopOrder stopOrder)
        {
            if (!_id_stopOrders.ContainsKey(stopOrder.StopOrderID)) return;
            _id_stopOrders[stopOrder.StopOrderID] = stopOrder;
        }
        #endregion

        #region Order
        public IEnumerable<Order> GetOrders(int accountID, int? fromID = null)
        {
            var list = _id_orders.Values.Where(r => r.AccountID == accountID);
            if (fromID != null) list = list.Where(r => r.OrderID >= fromID.Value);
            return list.ToList();
        }

        public IEnumerable<Order> GetOrders(IEnumerable<int> ids)
        {
            return _id_orders.Values.Where(r => ids.Contains(r.OrderID)).ToList();
        }

        public Order CreateOrder(int accountID, DateTime time, int insID, BuySell bs, int lotCount, decimal? price, OrderStatus status, int? stopOrderID, long orderNo)
        {
            Order ord = new Order()
            {
                OrderID = _nextOrderID++,
                AccountID = accountID,
                Time = time,
                InsID = insID,
                BuySell = bs,
                LotCount = lotCount,
                Price = price,
                Status = status,
                StopOrderID = stopOrderID,
                OrderNo = orderNo
            };
            _id_orders.Add(ord.OrderID, ord);

            return ord;
        }

        public void UpdateOrder(Order order)
        {
            if (!_id_orders.ContainsKey(order.OrderID)) return;
            _id_orders[order.OrderID] = order;
        }
        #endregion

        #region Trade
        public IEnumerable<Trade> GetTrades(int accountID, int? fromID = null)
        {
            var list = _id_trades.Values.Where(r => r.AccountID == accountID);
            if (fromID != null) list = list.Where(r => r.TradeID >= fromID.Value);
            return list.ToList();
        }

        public Trade CreateTrade(int accountID, int orderID, DateTime time, int insID, BuySell bs, int lotCount, decimal price, decimal comm, long tradeNo)
        {
            Trade trade = new Trade()
            {
                TradeID = _nextTradeID++,
                AccountID = accountID,
                OrderID = orderID,
                Time = time,
                InsID = insID,
                BuySell = bs,
                LotCount = lotCount,
                Price = price,
                Comm = comm,
                TradeNo = tradeNo
            };
            _id_trades.Add(trade.TradeID, trade);

            return trade;
        }
        #endregion

        #region Cash
        public Cash GetCash(int accountID)
        {
            return _id_cash.Values.FirstOrDefault(r => r.AccountID == accountID);
        }

        public Cash CreateCash(int accountID, decimal init, decimal curr, decimal sell, 
            decimal buy, decimal sellComm, decimal buyComm)
        {
            Cash cash = new Cash()
            {
                CashID = _nextCashID++,
                AccountID = accountID,
                Initial = init,
                Current = curr,
                Sell = sell,
                SellComm = sellComm,
                Buy = buy,
                BuyComm = buyComm
            };
            _id_cash.Add(cash.CashID, cash);

            return cash;
        }

        public void UpdateCash(Cash cash)
        {
            if (!_id_cash.ContainsKey(cash.CashID)) return;

            _id_cash[cash.CashID] = cash;
        }
        #endregion

        #region Holding
        public IEnumerable<Holding> GetHoldings(int accountID)
        {
            return _id_holdings.Values.Where(r => r.AccountID == accountID).ToList();
        }

        public Holding CreateHolding(int accountID, int insID, int lotCount)
        {
            Holding holding = new Holding()
            {
                HoldingID = _nextHoldingID++,
                AccountID = accountID,
                InsID = insID,
                LotCount = lotCount
            };
            _id_holdings.Add(holding.HoldingID, holding);

            return holding;
        }

        public void UpdateHolding(Holding holding)
        {
            if (!_id_holdings.ContainsKey(holding.HoldingID)) return;

            _id_holdings[holding.HoldingID] = holding;
        }

        public void DeleteHolding(int holdingID)
        {
            if (_id_holdings.ContainsKey(holdingID))
            {
                _id_holdings.Remove(holdingID);
            }
        }
        #endregion
    }
}
