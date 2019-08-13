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
        CommonData.Account CreateTestAccount(string code, string name, decimal commPerc, bool isShortEnable);
        CommonData.Cash CreateCash(int accountID, decimal initialSumma, decimal currSumma, decimal sellSumma, decimal buySumma, decimal sellCommSumma, decimal buyCommSumma);
        CommonData.Account GetAccountByID(int accountID);
        void UpdateAccount(CommonData.Account account);
        bool DeleteTestAccountData(int accountID, bool fullDelete);
        CommonData.Cash GetCash(int accountID);
        IEnumerable<CommonData.Holding> GetHoldings(int accountID);
        IEnumerable<Trade> GetTrades(int accountID, int? fromID = null);
        IEnumerable<Order> GetOrders(int accountID, int? fromID = null);
        IEnumerable<Order> GetOrders(IEnumerable<int> ids);
        IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID = null);
        IEnumerable<StopOrder> GetStopOrders(IEnumerable<int> ids);
        IEnumerable<Series> GetSeries(int accountID);
        IEnumerable<SeriesValue> GetValues(int seriesID, int skipCount = 0, int? takeCount = null);
        int GetValuesCount(int seriesID);
    }
}
