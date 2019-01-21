using Common;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using CommonData = Common.Data;

namespace Storage
{
    public class TickHistoryDA : ITickHistoryDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public TickHistoryDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Insert tick history data
        /// </summary>
        /// <param name="insID">Instrum id</param>
        /// <param name="date">date</param>
        /// <param name="data">Tick history data (all-trades format)</param>
        /// <returns>TickHistoryID</returns>
        public int InsertData(int insID, DateTime date, byte[] data)
        {
            var tickHistory = new CommonData.DbTickHistory()
            {
                Date = date,
                InsID = insID,
                Data = data
            };

            using (var db = new DaContext(_options))
            {
                db.DbTickHistory.Add(tickHistory);
                db.SaveChanges();
            }

            return tickHistory.TickHistoryID;
        }

        /// <summary>
        /// Delete tick history data by instrum id and date
        /// </summary>
        /// <param name="insID">Instrum id</param>
        /// <param name="date">History date</param>
        public void DeleteData(int insID, DateTime date)
        {
            using (var db = new DaContext(_options))
            {
                var hists = db.DbTickHistory.Where(r => r.InsID == insID && r.Date == date);
                if (hists.Any())
                {
                    foreach (var hist in hists)
                    {
                        db.DbTickHistory.Remove(hist);
                    }
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Get all history date
        /// </summary>
        /// <param name="insID">Instrum id</param>
        /// <returns>History date list</returns>
        public IEnumerable<DateTime> GetDates(int? insID)
        {
            List<DateTime> list = new List<DateTime>();

            using (var db = new DaContext(_options))
            {
                if (insID != null)
                {
                    list = db.DbTickHistory
                        .Where(r => r.InsID == insID.Value)
                        .Select(r => r.Date).Distinct().ToList();
                }
                else
                {
                    list = db.DbTickHistory
                        .Select(r => r.Date).Distinct().ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// Get all instrum ids
        /// </summary>
        /// <returns>Instrum Id list</returns>
        public IEnumerable<int> GetInstrums(DateTime? date)
        {
            List<int> list = new List<int>();

            using (var db = new DaContext(_options))
            {
                if (date != null)
                {
                    list = db.DbTickHistory
                        .Where(r => r.Date == date.Value)
                        .Select(r => r.InsID).Distinct().ToList();
                }
                else
                {
                    list = db.DbTickHistory
                        .Select(r => r.InsID).Distinct().ToList();
                }
            }

            return list;
        }

        /// <summary>
        /// Get tick history data by instrum id and date
        /// </summary>
        /// <param name="insID">Instrum id</param>
        /// <param name="date">History date</param>
        /// <returns>All trades binary data or null</returns>
        public byte[] GetData(int insID, DateTime date)
        {
            using (var db = new DaContext(_options))
            {
                var hist = db.DbTickHistory.FirstOrDefault(r => r.InsID == insID && r.Date == date);
                if (hist == null || hist.Data == null) return null;

                return hist.Data;
            }
        }
    }
}
