using Common.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITickHistoryBL
    {
        Task<IEnumerable<Tick>> GetTicksAsync(int insID, DateTime date);
        IEnumerable<Instrum> GetInstrumsByDate(DateTime date);
        IEnumerable<DateTime> GetDatesByInstrum(int insID);
    }
}
