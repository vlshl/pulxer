using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITickHistory
    {
        public Task<Tick[]> GetTicksAsync(DateTime date, int insId);
        public Task<bool> SaveTicksBlobAsync(DateTime date, string ticker, byte[] data);
        public DateTime[] GetDates(int year = 0);
        public DateTime? GetLastDate();
        public Task<Instrum[]> GetInstrumsAsync(DateTime date);
    }
}
