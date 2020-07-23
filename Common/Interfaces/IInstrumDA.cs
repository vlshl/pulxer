using CommonData = Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IInstrumDA
    {
        IEnumerable<CommonData.Instrum> GetInstrums();
        int InsertInstrum(string ticker, string shortName, string name, int lotSize, int decimals, decimal priceStep);
        void UpdateInstrum(int insID, string ticker, string shortName, string name, int lotSize, int decimals, decimal priceStep);
        CommonData.Instrum GetInstrum(int insID, string ticker = null);
        void DeleteInstrumByID(int insID);
        IEnumerable<CommonData.InstrumListItem> GetInstrumList();
    }
}
