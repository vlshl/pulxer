using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonData = Common.Data;

namespace Storage
{
    /// <summary>
    /// Trade account da-layer
    /// </summary>
    public class AccountDA : IAccountDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public AccountDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get Account by Id
        /// </summary>
        /// <param name="accountID">Id</param>
        /// <returns>Account data</returns>
        public Account GetAccountByID(int accountID)
        {
            Account account = null;

            using (var db = new DaContext(_options))
            {
                account = db.Account.Find(accountID);
            }

            return account;
        }

        /// <summary>
        /// Get all accounts
        /// </summary>
        /// <returns>Accounts list</returns>
        public IEnumerable<Account> GetAccounts()
        {
            using (var db = new DaContext(_options))
            {
                return db.Account.ToList();
            }
        }

        /// <summary>
        /// Insert new account
        /// </summary>
        /// <param name="code">Account code</param>
        /// <param name="name">Account full name</param>
        /// <param name="commPerc">Commission percent</param>
        /// <param name="isShortEnable">Is short position enable</param>
        /// <param name="accType">Account type (real or test)</param>
        /// <returns>New account id</returns>
        public Account CreateAccount(string code, string name, decimal commPerc, bool isShortEnable, AccountTypes accType)
        {
            Account account = new Account()
            {
                Code = code,
                Name = name,
                CommPerc = commPerc,
                IsShortEnable = isShortEnable,
                AccountType = accType
            };

            using (var db = new DaContext(_options))
            {
                db.Account.Add(account);
                db.SaveChanges();
            }

            return account;
        }

        /// <summary>
        /// Update account data
        /// </summary>
        /// <param name="account">Account data</param>
        public void UpdateAccount(Account account)
        {
            using (var db = new DaContext(_options))
            {
                db.Update(account);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get cash by account Id
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <returns>Cash</returns>
        public Cash GetCash(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                return db.Cash.FirstOrDefault(c => c.AccountID == accountID);
            }
        }

        /// <summary>
        /// Create cash record
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="init">Initial summa</param>
        /// <param name="curr">Current summa</param>
        /// <param name="sell">Sell summa</param>
        /// <param name="buy">Buy summa</param>
        /// <param name="sellComm">Sell commission summa</param>
        /// <param name="buyComm">Buy commission summa</param>
        /// <returns>New cash record</returns>
        public Cash CreateCash(int accountID, decimal init, decimal curr, decimal sell, decimal buy, decimal sellComm, decimal buyComm)
        {
            Cash cash = new Cash()
            {
                AccountID = accountID,
                Initial = init,
                Current = curr,
                Sell = sell,
                Buy = buy,
                SellComm = sellComm,
                BuyComm = buyComm
            };

            using (var db = new DaContext(_options))
            {
                db.Cash.Add(cash);
                db.SaveChanges();
            }

            return cash;
        }

        /// <summary>
        /// Update cash object
        /// </summary>
        /// <param name="cash">Cash object (CashID > 0)</param>
        public void UpdateCash(CommonData.Cash cash)
        {
            using (var db = new DaContext(_options))
            {
                db.Update(cash);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get Holdings by account Id
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <returns>Holdings list</returns>
        public IEnumerable<Holding> GetHoldings(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                return db.Holding.Where(r => r.AccountID == accountID).ToList();
            }
        }

        /// <summary>
        /// Create holding record
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="insID">Instrum id</param>
        /// <param name="lotCount">Lot count</param>
        /// <returns>Holding record</returns>
        public Holding CreateHolding(int accountID, int insID, int lotCount)
        {
            Holding holding = new Holding()
            {
                AccountID = accountID,
                InsID = insID,
                LotCount = lotCount
            };

            using (var db = new DaContext(_options))
            {
                db.Holding.Add(holding);
                db.SaveChanges();
            }

            return holding;
        }

        /// <summary>
        /// Update Holding object
        /// </summary>
        /// <param name="holding">Holding object</param>
        public void UpdateHolding(Holding holding)
        {
            using (var db = new DaContext(_options))
            {
                db.Update(holding);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete holding
        /// </summary>
        /// <param name="holdingID">Holding id</param>
        public void DeleteHolding(int holdingID)
        {
            using (var db = new DaContext(_options))
            {
                var holding = db.Holding.Find(holdingID);
                if (holding != null)
                {
                    db.Holding.Remove(holding);
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Get Order list by account Id
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <param name="fromID">Minimal order ID or null</param>
        /// <returns>Order list</returns>
        public IEnumerable<Order> GetOrders(int accountID, int? fromID = null)
        {
            using (var db = new DaContext(_options))
            {
                var orders = db.Order.Where(r => r.AccountID == accountID);
                if (fromID != null) orders = orders.Where(r => r.OrderID >= fromID.Value);

                return orders.ToList();
            }
        }

        /// <summary>
        /// Get Order list by IDs
        /// </summary>
        /// <param name="ids">OrderID list</param>
        /// <returns>Order list</returns>
        public IEnumerable<Order> GetOrders(IEnumerable<int> ids)
        {
            using (var db = new DaContext(_options))
            {
                return db.Order.Where(r => ids.Contains(r.OrderID)).ToList();
            }
        }

        /// <summary>
        /// Create order
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="time">Date and time</param>
        /// <param name="insID">Instrm id</param>
        /// <param name="bs">BuySell</param>
        /// <param name="lotCount">Lots count</param>
        /// <param name="price">Price or null</param>
        /// <param name="status">Order status</param>
        /// <param name="stopOrderID">Stop order id or null</param>
        /// <param name="orderNo">Global order number</param>
        /// <returns></returns>
        public Order CreateOrder(int accountID, DateTime time, int insID, BuySell bs, int lotCount, decimal? price, OrderStatus status, int? stopOrderID, long orderNo)
        {
            Order order = new Order()
            {
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

            using (var db = new DaContext(_options))
            {
                db.Order.Add(order);
                db.SaveChanges();
            }

            return order;
        }

        /// <summary>
        /// Update Order object
        /// </summary>
        /// <param name="order">Order object (OrderID > 0)</param>
        public void UpdateOrder(Order order)
        {
            using (var db = new DaContext(_options))
            {
                db.Update(order);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get StopOrder list by account Id
        /// </summary>
        /// <param name="accountID">account Id</param>
        /// <param name="fromID">Minimal stoporder ID or null</param>
        /// <returns>StopOrder list</returns>
        public IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID = null)
        {
            using (var db = new DaContext(_options))
            {
                var stopOrders = db.StopOrder.Where(r => r.AccountID == accountID);
                if (fromID != null) stopOrders = stopOrders.Where(r => r.StopOrderID >= fromID.Value);

                return stopOrders.ToList();
            }
        }

        /// <summary>
        /// Get StopOrder list by IDs
        /// </summary>
        /// <param name="ids">StopOrderID list</param>
        /// <returns>StopOrder list</returns>
        public IEnumerable<StopOrder> GetStopOrders(IEnumerable<int> ids)
        {
            using (var db = new DaContext(_options))
            {
                return db.StopOrder.Where(r => ids.Contains(r.StopOrderID)).ToList();
            }
        }

        /// <summary>
        /// Create stop-order
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="time">Date and time</param>
        /// <param name="insID">Instrum id</param>
        /// <param name="bs">Buy or sell</param>
        /// <param name="st">StopLoss or TakeProfit</param>
        /// <param name="endTime">End time or null</param>
        /// <param name="alertPrice">Alert price</param>
        /// <param name="price">Order price or null</param>
        /// <param name="lotCount">Lots count</param>
        /// <param name="status">Stop order status</param>
        /// <param name="completeTime">Complete date and time</param>
        /// <param name="stopOrderNo">Global stop-order number</param>
        /// <returns>New stop-order</returns>
        public StopOrder CreateStopOrder(int accountID, DateTime time, int insID, BuySell bs, StopOrderType st, DateTime? endTime, decimal alertPrice, decimal? price, int lotCount, 
            StopOrderStatus status, DateTime? completeTime, long stopOrderNo)
        {
            StopOrder stopOrder = new StopOrder()
            {
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

            using (var db = new DaContext(_options))
            {
                db.StopOrder.Add(stopOrder);
                db.SaveChanges();
            }

            return stopOrder;
        }

        /// <summary>
        /// Update StopOrder object
        /// </summary>
        /// <param name="stopOrder">StopOrder object (StopOrderID > 0)</param>
        public void UpdateStopOrder(StopOrder stopOrder)
        {
            using (var db = new DaContext(_options))
            {
                db.Update(stopOrder);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get trades
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="fromID">Minimal trade ID or null</param>
        /// <returns>Trades list</returns>
        public IEnumerable<Trade> GetTrades(int accountID, int? fromID = null)
        {
            using (var db = new DaContext(_options))
            {
                var trades = db.Trade.Where(r => r.AccountID == accountID);
                if (fromID != null) trades = trades.Where(r => r.TradeID >= fromID.Value);

                return trades.ToList();
            }
        }

        /// <summary>
        /// Create trade
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="orderID">Order id</param>
        /// <param name="time">Date and time</param>
        /// <param name="insID">Instrum id</param>
        /// <param name="bs">Buy or sell</param>
        /// <param name="lotCount">Lots count</param>
        /// <param name="price">Price</param>
        /// <param name="comm">Commission</param>
        /// <param name="tradeNo">Global trade number</param>
        /// <returns>New trade</returns>
        public Trade CreateTrade(int accountID, int orderID, DateTime time, int insID, BuySell bs, int lotCount, decimal price, decimal comm, long tradeNo)
        {
            Trade trade = new Trade()
            {
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

            using (var db = new DaContext(_options))
            {
                db.Trade.Add(trade);
                db.SaveChanges();
            }

            return trade;
        }

        #region Series
        public Series CreateSeries(int accountID, string key, string name, SeriesAxis axis, string data)
        {
            Series series = new Series()
            {
                SeriesID = 0,
                Key = key,
                AccountID = accountID,
                Name = name,
                Axis = axis,
                Data = data
            };

            using (var db = new DaContext(_options))
            {
                db.Series.Add(series);
                db.SaveChanges();
            }

            return series;
        }

        public void CreateSeriesValues(IEnumerable<SeriesValue> values)
        {
            using (var db = new DaContext(_options))
            {
                foreach (var v in values)
                {
                    db.SeriesValue.Add(v);
                }
                db.SaveChanges();
            }
        }

        public IEnumerable<Series> GetSeries(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                return db.Series.Where(r => r.AccountID == accountID).ToList();
            }
        }

        public IEnumerable<SeriesValue> GetSeriesValues(int seriesID)
        {
            using (var db = new DaContext(_options))
            {
                return db.SeriesValue.Where(r => r.SeriesID == seriesID).ToList();
            }
        }
        #endregion

        /// <summary>
        /// Delete all account data by accountID (trades, stoporders, orders, holdings, cashes)
        /// Account not delete
        /// </summary>
        /// <param name="accountID">AccountID</param>
        public void DeleteAccountData(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        db.Database.ExecuteSqlCommand(string.Format("delete from postrade where trade_id in (select trade_id from trade where account_id = {0})", accountID.ToString()));
                        db.Database.ExecuteSqlCommand(string.Format("delete from postrade where pos_id in (select pos_id from positions where account_id = {0})", accountID.ToString()));
                        db.Database.ExecuteSqlCommand("delete from positions where account_id = " + accountID.ToString());
                        db.Database.ExecuteSqlCommand("delete from trade where account_id = " + accountID.ToString());
                        db.Database.ExecuteSqlCommand("delete from stoporder where account_id = " + accountID.ToString());
                        db.Database.ExecuteSqlCommand("delete from orders where account_id = " + accountID.ToString());
                        db.Database.ExecuteSqlCommand("delete from holding where account_id = " + accountID.ToString());
                        db.Database.ExecuteSqlCommand("delete from cash where account_id = " + accountID.ToString());
                        db.Database.ExecuteSqlCommand("delete from seriesvalue where series_id in (select series_id from series where account_id = " + accountID.ToString() + ")");
                        db.Database.ExecuteSqlCommand("delete from series where account_id = " + accountID.ToString());

                        db.Database.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        db.Database.RollbackTransaction();
                        throw new Exception("Database error occurred while deleting account data", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Delete account, all account data (trades, orders, etc) must be deleted before
        /// </summary>
        /// <param name="accountID">AccountID</param>
        public void DeleteAccount(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                var account = db.Account.Find(accountID);
                if (account != null)
                {
                    db.Account.Remove(account);
                    db.SaveChanges();
                }
            }
        }
    }
}
