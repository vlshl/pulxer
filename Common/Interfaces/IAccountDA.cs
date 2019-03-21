using Common.Data;
using Platform;
using System;
using System.Collections.Generic;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// Account da-layer interface
    /// </summary>
    public interface IAccountDA
    {
        Account GetAccountByID(int accountID);
        IEnumerable<Account> GetAccounts();
        Account CreateAccount(string code, string name, decimal commPerc, bool isShortEnable, AccountTypes accType);
        void UpdateAccount(Account account);

        Cash GetCash(int accountID);
        Cash CreateCash(int accountID, decimal init, decimal curr, decimal sell, decimal buy, decimal sellComm, decimal buyComm);
        void UpdateCash(Cash cash);

        IEnumerable<Holding> GetHoldings(int accountID);
        Holding CreateHolding(int accountID, int insID, int lotCount);
        void UpdateHolding(Holding holding);
        void DeleteHolding(int holdingID);

        IEnumerable<Order> GetOrders(int accountID, int? fromID = null);
        IEnumerable<Order> GetOrders(IEnumerable<int> ids);
        Order CreateOrder(int accountID, DateTime time, int insID, BuySell bs, int lotCount, decimal? price,
            OrderStatus status, int? stopOrderID, long orderNo);
        void UpdateOrder(Order order);

        IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID = null);
        IEnumerable<StopOrder> GetStopOrders(IEnumerable<int> ids);
        StopOrder CreateStopOrder(int accountID, DateTime time, int insID, BuySell bs, StopOrderType st, DateTime? endTime, decimal alertPrice, decimal? price,
            int lotCount, StopOrderStatus status, DateTime? completeTime, long stopOrderNo);
        void UpdateStopOrder(StopOrder stopOrder);

        IEnumerable<Trade> GetTrades(int accountID, int? fromID = null);
        Trade CreateTrade(int accountID, int orderID, DateTime time, int insID, BuySell bs, int lotCount, decimal price, decimal comm, long tradeNo);

        void DeleteAccountData(int accountID);
        void DeleteAccount(int accountID);
    }
}
