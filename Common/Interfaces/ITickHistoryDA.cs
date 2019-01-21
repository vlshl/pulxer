using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface ITickHistoryDA
    {
        int InsertData(int insID, DateTime date, byte[] data);
        void DeleteData(int insID, DateTime date);
        IEnumerable<DateTime> GetDates(int? insID);
        IEnumerable<int> GetInstrums(DateTime? date);
        byte[] GetData(int insID, DateTime date);
    }
}
