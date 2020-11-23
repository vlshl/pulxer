using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Platform;
using Storage.DbModel;
using System.Linq;
using System.Xml.Linq;
using CommonData = Common.Data;

namespace Storage
{
    /// <summary>
    /// Data access layer for Chart object
    /// </summary>
    public class ChartDA : IChartDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public ChartDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get Chart object by Id
        /// </summary>
        /// <param name="chartId"></param>
        /// <returns></returns>
        public CommonData.Chart GetChartByID(int chartId)
        {
            DbChart chart = null;

            using (var db = new DaContext(_options))
            {
                chart = db.Chart.Find(chartId);
                if (chart == null) return null;
            }

            var xd = new XDocument();
            if (!string.IsNullOrEmpty(chart.Data))
            {
                xd = XDocument.Parse(chart.Data);
            }

            return new CommonData.Chart()
            {
                ChartID = chart.ChartID,
                InsID = chart.InsID,
                Tf = (Timeframes)chart.Tf,
                AccountID = chart.AccountID,
                XmlData = xd
            };
        }

        /// <summary>
        /// Get Chart object by account, instrument and timeframe (single chart)
        /// </summary>
        /// <param name="accountID"></param>
        /// <param name="insID"></param>
        /// <param name="tf"></param>
        /// <returns></returns>
        public CommonData.Chart GetChart(int accountID, int insID, Timeframes tf)
        {
            return null; //?????????????????????
            DbChart chart = null;

            using (var db = new DaContext(_options))
            {
                chart = db.Chart.FirstOrDefault(r => r.AccountID == accountID && r.InsID == insID && r.Tf == (byte)tf);
                if (chart == null) return null;
            }

            var xd = new XDocument();
            if (!string.IsNullOrEmpty(chart.Data))
            {
                xd = XDocument.Parse(chart.Data);
            }

            return new CommonData.Chart()
            {
                ChartID = chart.ChartID,
                InsID = chart.InsID,
                Tf = (Timeframes)chart.Tf,
                AccountID = chart.AccountID,
                XmlData = xd
            };
        }

        /// <summary>
        /// Create new chart
        /// </summary>
        /// <param name="accountID">Account Id</param>
        /// <param name="insID">Instrum Id</param>
        /// <param name="tf">Timeframe</param>
        /// <param name="xData">Chart data</param>
        /// <returns></returns>
        public int CreateChart(int accountID, int insID, Timeframes tf, XDocument xData)
        {
            DbChart db_chart = new DbChart()
            {
                ChartID = 0,
                InsID = insID,
                Tf = (byte)tf,
                AccountID = accountID,
                Data = xData.ToString()
            };

            using (var db = new DaContext(_options))
            {
                db.Chart.Add(db_chart);
                db.SaveChanges();
            }

            return db_chart.ChartID;
        }

        /// <summary>
        /// Update chart data
        /// </summary>
        /// <param name="chartID">Chart Id</param>
        /// <param name="xData">New chart data</param>
        public void UpdateChart(int chartID, XDocument xData)
        {
            using (var db = new DaContext(_options))
            {
                var chart = db.Chart.Find(chartID);
                if (chart == null) return;

                chart.Data = xData.ToString();
                db.Chart.Update(chart);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete Chart object from db
        /// </summary>
        /// <param name="chartID"></param>
        public void DeleteChartByID(int chartID)
        {
            using (var db = new DaContext(_options))
            {
                var chart = db.Chart.Find(chartID);
                if (chart == null) return;

                db.Remove(chart);
            }
        }
    }
}
