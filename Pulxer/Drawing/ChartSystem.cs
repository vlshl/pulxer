using Common.Interfaces;
using Platform;
using System.Collections.Generic;
using Common.Data;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System;

namespace Pulxer.Drawing
{
    public class ChartSystem
    {
        private readonly IChartDA _chartDA;
        private readonly IInstrumBL _instrumBL;
        private readonly IInsStoreBL _insStoreBL;
        private readonly IAccountDA _accountDA;
        private readonly IRepositoryBL _reposBL;
        private readonly ChartManagerCache _cmCache;
        private readonly ITickDispatcher _tickDisp;

        public ChartSystem(IChartDA chartDA, IInstrumBL instrumBL, IInsStoreBL insStoreBL, IAccountDA accountDA, IRepositoryBL reposBL, 
            ChartManagerCache cmCache, ITickDispatcher tickDisp)
        {
            _chartDA = chartDA;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _accountDA = accountDA;
            _reposBL = reposBL;
            _cmCache = cmCache;
            _tickDisp = tickDisp;
        }

        public ChartManager GetChartManager(int accountID, int insID, Timeframes tf)
        {
            ChartManager cm = _cmCache.GetChartManager(accountID, insID, tf);
            if (cm != null) return cm;

            var account = _accountDA.GetAccountByID(accountID);
            if (account == null) return null;

            if (account.AccountType == AccountTypes.Real)
            {
                cm = new ChartManager(_instrumBL, _insStoreBL, _accountDA, _tickDisp);
            }
            else
            {
                DateTime end = DateTime.Today;
                DateTime start = end.AddDays(-1);
                var json = _reposBL.GetStringParam(TestRun.ACCOUNT_META + accountID.ToString());
                try
                {
                    var meta = JsonConvert.DeserializeObject<AccountMeta>(json);
                    if (meta != null)
                    {
                        start = meta.TickSource_StartDate;
                        end = meta.TickSource_EndDate;
                    }
                }
                catch { }
                cm = new ChartManager(_instrumBL, _insStoreBL, _accountDA, start, end);
            }

            var chart = _chartDA.GetChart(accountID, insID, tf);
            if (chart != null)
            {
                cm.Initialize(chart.XmlData);
            }
            else
            {
                cm.Initialize();
                cm.CreatePrices(insID, tf);
            }

            // await cm.LoadHistoryAsync();

            _cmCache.AddChartManager(accountID, insID, tf, cm);

            return cm;
        }
    }
}
