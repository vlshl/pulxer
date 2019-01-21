using Common.Data;
using Platform;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface ILeechDA
    {
        IEnumerable<Instrum> GetInstrumList();
        IEnumerable<Account> GetAccountList();
        IEnumerable<StopOrder> GetStopOrderList(int accountID);
        IEnumerable<Order> GetOrderList(int accountID);
        IEnumerable<Trade> GetTradeList(int accountID);
        Cash GetCash(int accountID);
        IEnumerable<Holding> GetHoldingList(int accountID);
    }
}
