using Platform;
using System.Collections.Generic;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// Account subsystem interface
    /// </summary>
    public interface IAccountBL
    {
        IEnumerable<CommonData.AccountListItem> GetAccountList();
        CommonData.Account CreateAccount(string code, string name, decimal commPerc, bool isShortEnable, decimal initialSumma, bool isTestAccount);
        CommonData.Account GetAccountByID(int accountID);
        void UpdateAccount(CommonData.Account account);

        CommonData.Cash GetCash(int accountID);
        IEnumerable<CommonData.Holding> GetHoldings(int accountID);

        IEnumerable<Trade> GetTrades(int accountID, int? fromID);

        IEnumerable<Order> GetOrders(int accountID, int? fromID);
        IEnumerable<Order> GetOrders(IEnumerable<int> ids);

        IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID);
        IEnumerable<StopOrder> GetStopOrders(IEnumerable<int> ids);
    }
}
