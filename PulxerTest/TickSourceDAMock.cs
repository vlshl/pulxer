using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulxerTest
{
    public class TickSourceDAMock : ITickSourceDA
    {
        private Dictionary<int, DbTickSource> _id_dbTickSource = new Dictionary<int, DbTickSource>();

        public void DeleteTickSourceByID(int tickSourceID)
        {
            if (_id_dbTickSource.ContainsKey(tickSourceID))
            {
                _id_dbTickSource.Remove(tickSourceID);
            }
        }

        public DbTickSource GetTickSourceByID(int tickSourceID)
        {
            if (_id_dbTickSource.ContainsKey(tickSourceID))
                return _id_dbTickSource[tickSourceID];
            else
                return null;
        }

        public IEnumerable<DbTickSource> GetTickSources()
        {
            return _id_dbTickSource.Values.ToList();
        }

        public int InsertTickSource(DbTickSource tickSource)
        {
            if (tickSource.TickSourceID != 0) return 0;

            int id = 0;
            if (_id_dbTickSource.Keys.Any())
            {
                id = _id_dbTickSource.Keys.Max() + 1;
            }
            else
            {
                id = 1;
            }

            tickSource.TickSourceID = id;
            _id_dbTickSource.Add(id, tickSource);

            return tickSource.TickSourceID;
        }

        public void UpdateTickSource(DbTickSource tickSource)
        {
            if (tickSource.TickSourceID <= 0) return;
            if (!_id_dbTickSource.Keys.Contains(tickSource.TickSourceID)) return;

            _id_dbTickSource[tickSource.TickSourceID] = tickSource;
        }
    }
}
