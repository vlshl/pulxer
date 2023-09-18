using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using CommonData = Common.Data;
using Common.Interfaces;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Common.Data;
using NpgsqlTypes;
using Storage.DbModel;
using Platform;

namespace Storage
{
    /// <summary>
    /// Fin. instrument data storage da-layer
    /// </summary>
    public class InsStoreDA : IInsStoreDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public InsStoreDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Create InsStore (array of fin. instrument quotes)
        /// </summary>
        /// <param name="insID">Instrument</param>
        /// <param name="tf">Timeframe</param>
        /// <param name="isEnable">Active for sync</param>
        /// <returns></returns>
        public int CreateInsStore(int insID, Timeframes tf, bool isEnable)
        {
            var insStore = new CommonData.InsStore()
            {
                InsID = insID,
                Tf = tf,
                IsEnable = isEnable
            };

            using (var db = new DaContext(_options))
            {
                db.InsStore.Add(insStore);
                db.SaveChanges();
            }

            return insStore.InsStoreID;
        }

        /// <summary>
        /// Get InsStore by Id
        /// </summary>
        /// <param name="insStoreID"></param>
        /// <returns></returns>
        public CommonData.InsStore GetInsStoreByID(int insStoreID)
        {
            CommonData.InsStore insStore = null;

            using (var db = new DaContext(_options))
            {
                insStore = db.InsStore.Find(insStoreID);
            }

            return insStore;
        }

        /// <summary>
        /// Get InsStore by instrument and timeframe
        /// </summary>
        /// <param name="insID"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        public CommonData.InsStore GetInsStore(int insID, Timeframes tf)
        {
            using (var db = new DaContext(_options))
            {
                return db.InsStore.FirstOrDefault(s => s.InsID == insID && s.Tf == tf);
            }
        }

        /// <summary>
        /// Get InsStore list by instrument
        /// </summary>
        /// <param name="insID">Instrument or null</param>
        /// <param name="timeframes">Timeframe or null</param>
        /// <param name="isEnable">IsEnable or null</param>
        /// <returns></returns>
        public IEnumerable<CommonData.InsStore> GetInsStores(int? insID, Timeframes? tf, bool? isEnabled)
        {
            List<CommonData.InsStore> insStores = new List<CommonData.InsStore>();

            using (var db = new DaContext(_options))
            {
                var list = db.InsStore.AsQueryable<InsStore>();
                if (insID != null) list = list.Where(r => r.InsID == insID.Value);
                if (tf != null) list = list.Where(r => r.Tf == tf.Value);
                if (isEnabled != null) list = list.Where(r => r.IsEnable == isEnabled.Value);
                insStores = list.ToList();
            }

            return insStores;
        }

        /// <summary>
        /// Update insStore
        /// </summary>
        /// <param name="insStoreID">ID</param>
        /// <param name="isEnable">IsEnable</param>
        public void UpdateInsStore(int insStoreID, bool isEnable)
        {
            using (var db = new DaContext(_options))
            {
                var isr = db.InsStore.Find(insStoreID);
                if (isr != null)
                {
                    isr.IsEnable = isEnable;
                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Store price bars to InsStore
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <param name="bars">Bars array</param>
        /// <param name="date1">First date (ignore time)</param>
        /// <param name="date2">Last date (ignore time)</param>
        /// <param name="cancel">Cancel object (use for cancel continuous operation)</param>
        public void InsertBars(int insStoreID, IEnumerable<DbBarHistory> bars, DateTime date1, DateTime date2, CancellationToken cancel)
        {
            if (bars == null) return;

            if (date2 == DateTime.MaxValue) date2 = date2.AddDays(-1);

            using (var db = new DaContext(_options))
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        int t1 = StorageLib.ToDbTime(date1.Date);
                        int t2 = StorageLib.ToDbTime(date2.Date.AddDays(1));

                        var insStoreParam = new NpgsqlParameter("@InsStoreID", insStoreID);
                        var time1Param = new NpgsqlParameter("@Time1", t1);
                        var time2Param = new NpgsqlParameter("@Time2", t2);

                        db.Database.ExecuteSqlRaw("delete from barhistory where insstore_id = @InsStoreID and bar_time >= @Time1 and bar_time < @Time2",
                            insStoreParam, time1Param, time2Param);

                        var timeParam = new NpgsqlParameter("@Time", NpgsqlDbType.Integer);
                        var opParam = new NpgsqlParameter("@OpenPrice", NpgsqlDbType.Integer);
                        var cpParam = new NpgsqlParameter("@CloseDelta", NpgsqlDbType.Smallint);
                        var hpParam = new NpgsqlParameter("@HighDelta", NpgsqlDbType.Smallint);
                        var lpParam = new NpgsqlParameter("@LowDelta", NpgsqlDbType.Smallint);
                        var vParam = new NpgsqlParameter("@Volume", NpgsqlDbType.Integer);

                        foreach (var bar in bars)
                        {
                            if (cancel.IsCancellationRequested) break;
                            if ((bar.Time < t1) || (bar.Time >= t2)) continue;

                            timeParam.Value = bar.Time;
                            opParam.Value = bar.OpenPrice;
                            cpParam.Value = bar.CloseDelta;
                            hpParam.Value = bar.HighDelta;
                            lpParam.Value = bar.LowDelta;
                            vParam.Value = bar.Volume;

                            db.Database.ExecuteSqlRaw("insert into barhistory (insstore_id, bar_time, open, close_d, high_d, low_d, volume) values (@InsStoreID, @Time, @OpenPrice, @CloseDelta, @HighDelta, @LowDelta, @Volume)",
                                insStoreParam, timeParam, opParam, cpParam, hpParam, lpParam, vParam);
                        }

                        db.Database.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        db.Database.RollbackTransaction();
                        throw new Exception("Database error occurred while inserting bars", ex);
                    }
                }
            }
        }

        /// <summary>
        /// Delete price bars from db
        /// </summary>
        /// <param name="insStoreID">InsStore</param>
        /// <param name="date1">First date (without time)</param>
        /// <param name="date2">Last date (without time)</param>
        public void DeleteBars(int insStoreID, DateTime date1, DateTime date2)
        {
            if (date2 == DateTime.MaxValue) date2 = date2.AddDays(-1);

            using (var db = new DaContext(_options))
            {
                try
                {
                    var insStoreParam = new NpgsqlParameter("@InsStoreID", insStoreID);
                    var time1Param = new NpgsqlParameter("@Time1", StorageLib.ToDbTime(date1.Date));
                    var time2Param = new NpgsqlParameter("@Time2", StorageLib.ToDbTime(date2.Date.AddDays(1)));

                    db.Database.ExecuteSqlRaw("delete from barhistory where insstore_id = @InsStoreID and bar_time >= @Time1 and bar_time < @Time2",
                        insStoreParam, time1Param, time2Param);
                }
                catch (Exception ex)
                {
                    throw new Exception("Database error occurred while deleting bars", ex);
                }
            }
        }

        /// <summary>
        /// Get historical data
        /// </summary>
        /// <param name="insStoreID">InsStote Id</param>
        /// <param name="date1">First date (without time)</param>
        /// <param name="date2">Last date (without time)</param>
        /// <returns>Async task</returns>
        public Task<IEnumerable<DbBarHistory>> GetHistoryAsync(int insStoreID, DateTime date1, DateTime date2)
        {
            if (date2 == DateTime.MaxValue) date2 = date2.AddDays(-1);

            return Task.Run<IEnumerable<DbBarHistory>>(() =>
            {
                int d1 = StorageLib.ToDbTime(date1.Date);
                int d2 = StorageLib.ToDbTime(date2.AddDays(1).Date);

                using (var db = new DaContext(_options))
                {
                    var list = db.DbBarHistory
                        .Where(r => r.InsStoreID == insStoreID && r.Time >= d1 && r.Time < d2)
                        .OrderBy(r => r.Time)
                        .ToList();

                    return list;
                }
            });
        }

        /// <summary>
        /// Get all InsStore periods by InsStoreId
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <returns>Periods</returns>
        public IEnumerable<InsStorePeriod> GetPeriods(int insStoreID)
        {
            List<InsStorePeriods> periods = new List<InsStorePeriods>();

            using (var db = new DaContext(_options))
            {
                periods = db.InsStorePeriods.Where(s => s.InsStoreID == insStoreID).ToList();

                return periods.Select(r => new InsStorePeriod(
                        r.StartDate,
                        r.EndDate,
                        r.IsLastDirty)).ToList();
            }
        }

        /// <summary>
        /// Update InsStore periods (delete old data and insert new data)
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <param name="periods">InsStore periods</param>
        public void UpdatePeriods(int insStoreID, IEnumerable<Common.InsStorePeriod> periods)
        {
            using (var db = new DaContext(_options))
            {
                db.Database.ExecuteSqlRaw("delete from periods where insstore_id = " + insStoreID.ToString());

                foreach (var p in periods)
                {
                    InsStorePeriods per = new InsStorePeriods()
                    {
                        InsStoreID = insStoreID,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        IsLastDirty = p.IsLastDirty
                    };
                    db.InsStorePeriods.Add(per);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get free days (weekends)
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <returns>Weekends (without time)</returns>
        public IEnumerable<DateTime> GetFreeDays(int insStoreID)
        {
            List<InsStoreFreeDays> freeDays = new List<InsStoreFreeDays>();

            using (var db = new DaContext(_options))
            {
                freeDays = db.InsStoreFreeDays.Where(s => s.InsStoreID == insStoreID).ToList();

                return freeDays.Select(r => r.Date).ToList();
            }
        }

        /// <summary>
        /// Update free days data (delete old free days and insert new free days)
        /// </summary>
        /// <param name="insStoreID">InsStore Id</param>
        /// <param name="freeDays">Free days list</param>
        public void UpdateFreeDays(int insStoreID, IEnumerable<DateTime> freeDays)
        {
            using (var db = new DaContext(_options))
            {
                db.Database.ExecuteSqlRaw("delete from freedays where insstore_id = " + insStoreID.ToString());

                foreach (var p in freeDays)
                {
                    InsStoreFreeDays fds = new InsStoreFreeDays()
                    {
                        InsStoreID = insStoreID,
                        Date = p.Date
                    };
                    db.InsStoreFreeDays.Add(fds);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete InsStore by Id
        /// </summary>
        /// <param name="insStoreID">InsStore ID</param>
        public void DeleteInsStoreByID(int insStoreID)
        {
            using (var db = new DaContext(_options))
            {
                var insStore = db.InsStore.Find(insStoreID);
                if (insStore != null)
                {
                    db.Remove(insStore);
                    db.SaveChanges();
                }
            }
        }
    }
}
