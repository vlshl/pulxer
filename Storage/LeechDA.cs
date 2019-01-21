using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Platform;
using Storage.DbModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Storage
{
    public class LeechDA : ILeechDA
    {
        private readonly DbContextOptions<LeechDAContext> _options;

        public LeechDA(DbContextOptions<LeechDAContext> options)
        {
            _options = options;
        }

        public IEnumerable<Instrum> GetInstrumList()
        {
            using (var db = new LeechDAContext(_options))
            {
                return db.Instrum.ToList();
            }
        }

        public IEnumerable<Account> GetAccountList()
        {
            using (var db = new LeechDAContext(_options))
            {
                return db.Account.ToList();
            }
        }

        public IEnumerable<StopOrder> GetStopOrderList(int accountID)
        {
            using (var db = new LeechDAContext(_options))
            {
                var list = db.StopOrder.Where(r => r.AccountID == accountID);

                return list.Select(r => new StopOrder()
                {
                    StopOrderID = r.StopOrderID,
                    Time = StorageLib.ToDateTime(r.Time),
                    InsID = r.InsID,
                    BuySell = (BuySell)r.BuySell,
                    StopType = (StopOrderType)r.StopType,
                    EndTime = StorageLib.ToDateTime(r.EndTime),
                    AlertPrice = r.AlertPrice,
                    Price = r.Price,
                    LotCount = r.LotCount,
                    Status = (StopOrderStatus)r.Status,
                    AccountID = r.AccountID,
                    CompleteTime = StorageLib.ToDateTime(r.CompleteTime),
                    StopOrderNo = r.StopOrderNo
                }).ToList();
            }
        }

        public IEnumerable<Order> GetOrderList(int accountID)
        {
            using (var db = new LeechDAContext(_options))
            {
                var list = db.Order.Where(r => r.AccountID == accountID);

                return list.Select(r => new Order()
                {
                    OrderID = r.OrderID,
                    Time = StorageLib.ToDateTime(r.Time),
                    InsID = r.InsID,
                    BuySell = (BuySell)r.BuySell,
                    LotCount = r.LotCount,
                    Price = r.Price,
                    Status = (OrderStatus)r.Status,
                    AccountID = r.AccountID,
                    StopOrderID = r.StopOrderID,
                    OrderNo = r.OrderNo
                }).ToList();
            }
        }

        public IEnumerable<Trade> GetTradeList(int accountID)
        {
            using (var db = new LeechDAContext(_options))
            {
                var list = db.Trade.Where(r => r.AccountID == accountID);

                return list.Select(r => new Trade()
                {
                    TradeID = r.TradeID,
                    OrderID = r.OrderID,
                    Time = StorageLib.ToDateTime(r.Time),
                    InsID = r.InsID,
                    BuySell = (BuySell)r.BuySell,
                    LotCount = r.LotCount,
                    Price = r.Price,
                    AccountID = r.AccountID,
                    Comm = r.Comm,
                    TradeNo = r.TradeNo
                }).ToList();
            }
        }

        public Cash GetCash(int accountID)
        {
            using (var db = new LeechDAContext(_options))
            {
                return db.Cash.FirstOrDefault(r => r.AccountID == accountID);
            }
        }

        public IEnumerable<Holding> GetHoldingList(int accountID)
        {
            using (var db = new LeechDAContext(_options))
            {
                return db.Holding.Where(r => r.AccountID == accountID).ToList();
            }
        }
    }
}
