using Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// InsStore da-layer interface
    /// </summary>
    public interface IInsStoreDA
    {
        int CreateInsStore(int insID, Timeframes tf, bool isEnable);
        CommonData.InsStore GetInsStoreByID(int insStoreID);
        CommonData.InsStore GetInsStore(int insID, Timeframes tf);
        IEnumerable<CommonData.InsStore> GetInsStores(int? insID, Timeframes? tf, bool? isEnabled);
        void UpdateInsStore(int insStoreID, bool isEnable);
        void DeleteInsStoreByID(int insStoreID);
        void InsertBars(int insStoreID, IEnumerable<CommonData.DbBarHistory> bars, DateTime date1, DateTime date2, CancellationToken cancel);
        void DeleteBars(int insStoreID, DateTime date1, DateTime date2);
        Task<IEnumerable<CommonData.DbBarHistory>> GetHistoryAsync(int insStoreID, DateTime date1, DateTime date2);
        IEnumerable<InsStorePeriod> GetPeriods(int insStoreID);
        void UpdatePeriods(int insStoreID, IEnumerable<Common.InsStorePeriod> periods);
        IEnumerable<DateTime> GetFreeDays(int insStoreID);
        void UpdateFreeDays(int insStoreID, IEnumerable<DateTime> freeDays);
    }
}
