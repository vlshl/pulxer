using Common.Interfaces;
using System.Collections.Generic;

namespace Pulxer.Drawing
{
    public class ChartSystem
    {
        private Dictionary<string, ChartManager> _key_chartManager = null;
        private readonly IChartDA _chartDA;
        private readonly IInstrumBL _instrumBL;
        private readonly IInsStoreBL _insStoreBL;
        private readonly IAccountDA _accountDA;

        public ChartSystem(IChartDA chartDA, IInstrumBL instrumBL, IInsStoreBL insStoreBL, IAccountDA accountDA)
        {
            _chartDA = chartDA;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _accountDA = accountDA;
            _key_chartManager = new Dictionary<string, ChartManager>();
        }

        //public ChartManager GetChartManager(int accountID, int insID, Timeframes tf)
        //{
        //    string key = accountID.ToString()
        //        + ":" + insID.ToString()
        //        + ":" + ((byte)tf).ToString();
        //    if (_key_chartManager.ContainsKey(key)) return _key_chartManager[key];

        //    ChartManager cm = new ChartManager(_instrumBL, _insStoreBL, _accountDA, _startDate, _endDate, _depManager);
        //    var chart = _chartDA.GetChart(accountID, insID, tf);
        //    if (chart != null)
        //    {
        //        cm.Initialize(chart.XmlData);
        //    }
        //    else
        //    {
        //        cm.Initialize();
        //        cm.CreatePrices(insID, tf);
        //    }
        //    _key_chartManager.Add(key, cm);

        //    return cm;
        //}
    }
}
