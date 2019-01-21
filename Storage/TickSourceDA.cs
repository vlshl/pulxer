using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Storage
{
    /// <summary>
    /// TickSource da-layer
    /// </summary>
    public class TickSourceDA : ITickSourceDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public TickSourceDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get TickSource by Id
        /// </summary>
        /// <param name="tickSourceID">Id</param>
        /// <returns>TickSiurce</returns>
        public DbTickSource GetTickSourceByID(int tickSourceID)
        {
            using (var db = new DaContext(_options))
            {
                return db.TickSource.Find(tickSourceID);
            }
        }

        /// <summary>
        /// Get all tickSources
        /// </summary>
        /// <returns>TickSources list</returns>
        public IEnumerable<DbTickSource> GetTickSources()
        {
            using (var db = new DaContext(_options))
            {
                return db.TickSource.ToList();
            }
        }

        /// <summary>
        /// Insert new tickSource
        /// </summary>
        /// <param name="tickSource">TickSource</param>
        /// <returns>New tickSource id</returns>
        public int InsertTickSource(DbTickSource tickSource)
        {
            using (var db = new DaContext(_options))
            {
                db.TickSource.Add(tickSource);
                db.SaveChanges();
            }

            return tickSource.TickSourceID;
        }

        /// <summary>
        /// Update tickSource
        /// </summary>
        /// <param name="tickSource">TickSource</param>
        public void UpdateTickSource(DbTickSource tickSource)
        {
            using (var db = new DaContext(_options))
            {
                db.Update(tickSource);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete TickSource by Id
        /// </summary>
        /// <param name="tickSourceID">Id</param>
        public void DeleteTickSourceByID(int tickSourceID)
        {
            using (var db = new DaContext(_options))
            {
                var ts = db.TickSource.Find(tickSourceID);
                if (ts != null)
                {
                    db.TickSource.Remove(ts);
                    db.SaveChanges();
                }
            }
        }
    }
}
