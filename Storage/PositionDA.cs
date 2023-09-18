using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Platform;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class PositionDA : IPositionDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public PositionDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get positions by account
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="isOpened">true-opened positions only, false - close positions only, null - all positions</param>
        /// <returns>Positions list</returns>
        public IEnumerable<Position> GetPositions(int accountID, bool? isOpened)
        {
            using (var db = new DaContext(_options))
            {
                var list = db.Position.Where(r => r.AccountID == accountID);
                if (isOpened == true) 
                    list = list.Where(r => !r.CloseTime.HasValue);
                else if (isOpened == false) 
                    list = list.Where(r => r.CloseTime.HasValue);

                return list.ToList();
            }
        }

        /// <summary>
        /// Get positions by ids
        /// </summary>
        /// <param name="ids">Id list</param>
        /// <returns>Positions list</returns>
        public IEnumerable<Position> GetPositions(IEnumerable<int> ids)
        {
            if (ids == null) ids = new List<int>();

            using (var db = new DaContext(_options))
            {
                return db.Position.Where(r => ids.Contains(r.PosID)).ToList();
            }
        }

        /// <summary>
        /// Get position trades
        /// </summary>
        /// <param name="posIDs">Position ID list</param>
        /// <returns>Position trades list</returns>
        public IEnumerable<PosTrade> GetPosTrades(IEnumerable<int> posIDs)
        {
            using (var db = new DaContext(_options))
            {
                return db.PosTrade.Where(r => posIDs.Contains(r.PosID)).ToList();
            }
        }

        /// <summary>
        /// Create new position
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <param name="insID">Instrum id</param>
        /// <param name="posType">Pos type (short or long)</param>
        /// <param name="openTime">Position open time</param>
        /// <param name="openPrice">Position open price</param>
        /// <param name="count">Instrum count (not lots)</param>
        /// <param name="closeTime">Close time or null</param>
        /// <param name="closePrice">Close price or null</param>
        /// <returns>New position</returns>
        public Position CreatePosition(int accountID, int insID, PosTypes posType, DateTime openTime, decimal openPrice, int count, DateTime? closeTime, decimal? closePrice)
        {
            Position pos = new Position()
            {
                PosID = 0,
                AccountID = accountID,
                InsID = insID,
                PosType = posType,
                OpenTime = openTime,
                OpenPrice = openPrice,
                Count = count,
                CloseTime = closeTime,
                ClosePrice = closePrice
            };

            using (var db = new DaContext(_options))
            {
                db.Position.Add(pos);
                db.SaveChanges();
            }

            return pos;
        }

        /// <summary>
        /// Update position
        /// </summary>
        /// <param name="pos">Position object</param>
        public void UpdatePosition(Position pos)
        {
            using (var db = new DaContext(_options))
            {
                db.Update<Position>(pos);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get PosTrade relations for open positions only
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <returns>PosTrade list</returns>
        public IEnumerable<PosTrade> GetOpenPosTrades(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                var posTrades = from r in db.PosTrade
                                join p in db.Position on r.PosID equals p.PosID
                                where !p.CloseTime.HasValue && p.AccountID == accountID
                                select new PosTrade(p.PosID, r.TradeID);

                return posTrades.ToList();
            }
        }

        /// <summary>
        /// Add PosTrade relation
        /// </summary>
        /// <param name="posID">Position id</param>
        /// <param name="tradeID">Trade id</param>
        public void AddPosTrade(int posID, int tradeID)
        {
            PosTrade pt = new PosTrade(posID, tradeID);

            using (var db = new DaContext(_options))
            {
                db.PosTrade.Add(pt);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Get new trades (without PosTrade reference)
        /// </summary>
        /// <param name="accountID">Account id</param>
        /// <returns>New trades list</returns>
        public IEnumerable<Trade> GetNewTrades(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                var trades =
                from t in db.Trade
                where t.AccountID == accountID && !db.PosTrade.Any(r => r.TradeID == t.TradeID)
                select t;

                return trades.ToList();
            }
        }

        /// <summary>
        /// Delete all positions and postrades
        /// </summary>
        /// <param name="accountID">Account id</param>
        public void DeleteAllPositions(int accountID)
        {
            using (var db = new DaContext(_options))
            {
                using (var trans = db.Database.BeginTransaction())
                {
                    try
                    {
                        var accountParam = new NpgsqlParameter("@AccountID", accountID);

                        db.Database.ExecuteSqlRaw("delete from postrade where pos_id in (select pos_id from positions where account_id = @AccountID)", accountParam);
                        db.Database.ExecuteSqlRaw("delete from positions where account_id = @AccountID", accountParam);

                        db.Database.CommitTransaction();
                    }
                    catch (Exception ex)
                    {
                        db.Database.RollbackTransaction();
                        throw new Exception("Database error occurred while deleting positions", ex);
                    }
                }
            }
        }
    }
}
