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
        Task<StopOrder[]> GetStopOrderList(int accountID);
        Task<Order[]> GetOrderList(int accountID);
        Task<Trade[]> GetTradeList(int accountID);
        Task<Cash> GetCash(int accountID);
        Task<Holding[]> GetHoldingList(int accountID);
    }
}
