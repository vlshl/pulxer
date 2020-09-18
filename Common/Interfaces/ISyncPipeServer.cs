using Common.Data;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ISyncPipeServer
    {
        Task<Instrum[]> GetInstrumList();
        Task<Account[]> GetAccountList();
        Task<StopOrder[]> GetStopOrders(int rAccountID, int fromId);
        Task<StopOrder[]> GetStopOrders(int[] ids);
        Task<Order[]> GetOrders(int rAccountID, int fromId);
        Task<Order[]> GetOrders(int[] ids);
        Task<Trade[]> GetTrades(int rAccountID, int fromId);
        Task<Cash> GetCash(int rAccountID);
        Task<Holding[]> GetHoldingList(int rAccountID);
    }
}
