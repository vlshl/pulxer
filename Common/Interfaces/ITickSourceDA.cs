using Common.Data;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface ITickSourceDA
    {
        DbTickSource GetTickSourceByID(int tickSourceID);
        IEnumerable<DbTickSource> GetTickSources();
        int InsertTickSource(DbTickSource tickSource);
        void UpdateTickSource(DbTickSource tickSource);
        void DeleteTickSourceByID(int tickSourceID);
    }
}
