using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Common.Data;
using System.Linq;
using Common;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    /// <summary>
    /// Instrum da-layer
    /// </summary>
    public class InstrumDA : IInstrumDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public InstrumDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get all instrums
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Instrum> GetInstrums()
        {
            using (var db = new DaContext(_options))
            {
                return db.Instrum.ToList();
            }
        }

        /// <summary>
        /// Get all instruments
        /// </summary>
        /// <returns>InstrumListItem array</returns>
        public IEnumerable<InstrumListItem> GetInstrumList()
        {
            using (var db = new DaContext(_options))
            {
                var list = (from s in db.Instrum
                            select new InstrumListItem(s.InsID, s.Ticker, s.ShortName)).ToList();

                return list;
            }
        }

        /// <summary>
        /// Get instrument by id or ticker
        /// </summary>
        /// <param name="insID">Instrument id or 0</param>
        /// <param name="ticker">Ticker or null/empty string</param>
        /// <returns></returns>
        public Instrum GetInstrum(int insID, string ticker = null)
        {
            Instrum ins = null;

            using (var db = new DaContext(_options))
            {
                if (insID > 0)
                    ins = db.Instrum.Find(insID);
                else if (ticker != null && ticker.Length > 0)
                    ins = db.Instrum.FirstOrDefault(s => s.Ticker == ticker);
            }

            return ins;
        }

        /// <summary>
        /// Insert new instrum to db
        /// </summary>
        /// <param name="ticker">Ticker</param>
        /// <param name="shortName">Short name</param>
        /// <param name="name">Full name</param>
        /// <param name="lotSize">Lot size</param>
        /// <param name="decimals">Decimals</param>
        /// <param name="priceStep">Price step</param>
        /// <returns>New instrum identity</returns>
        public int InsertInstrum(string ticker, string shortName, string name, int lotSize, int decimals, decimal priceStep)
        {
            Instrum ins = new Instrum()
            {
                Ticker = ticker,
                ShortName = shortName,
                Name = name,
                LotSize = lotSize,
                Decimals = decimals,
                PriceStep = priceStep
            };

            using (var db = new DaContext(_options))
            {
                db.Instrum.Add(ins);
                db.SaveChanges();
            }

            return ins.InsID;
        }

        /// <summary>
        /// Update instrum
        /// </summary>
        /// <param name="insID">ID</param>
        /// <param name="ticker">Ticker</param>
        /// <param name="shortName">Short name</param>
        /// <param name="name">Full name</param>
        /// <param name="lotSize">Lot size</param>
        /// <param name="decimals">Decimals</param>
        /// <param name="priceStep">Price step</param>
        public void UpdateInstrum(int insID, string ticker, string shortName, string name, int lotSize, int decimals,
            decimal priceStep)
        {
            using (var db = new DaContext(_options))
            {
                var instrum = db.Instrum.Find(insID);
                if (instrum != null)
                {
                    instrum.Ticker = ticker;
                    instrum.ShortName = shortName;
                    instrum.Name = name;
                    instrum.LotSize = lotSize;
                    instrum.Decimals = decimals;
                    instrum.PriceStep = priceStep;

                    db.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Delete Instrum object by Id
        /// </summary>
        /// <param name="insID">Id</param>
        public void DeleteInstrumByID(int insID)
        {
            using (var db = new DaContext(_options))
            {
                var ins = db.Instrum.Find(insID);
                if (ins != null)
                {
                    db.Instrum.Remove(ins);
                    db.SaveChanges();
                }
            }
        }
    }
}
