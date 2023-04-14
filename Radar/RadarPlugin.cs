using Common;
using Common.Interfaces;
using Indic;
using Platform;
using System;
using System.Collections.Generic;

namespace Radar
{
    public class RadarPlugin : IPxPlugin
    {
        private PxColumn[] cols;
        private readonly IPluginPlatform _platform;
        private IInstrum _gazp, _rosn;
        private IBarRow _gazp_bars, _rosn_bars;
        private Ma _gazp_ma10, _rosn_ma10;
        private decimal _gazp_lastPrice, _rosn_lastPrice;
        private RadarRow _gr;
        private RadarRow _rr;

        public RadarPlugin(IPluginPlatform platform)
        {
            _platform = platform;
            _gazp_lastPrice = 0;
            _rosn_lastPrice = 0;

            cols = new PxColumn[5];
            cols[0] = new PxColumn("ticker", "Тикер");
            cols[1] = new PxColumn("name", "Инструмент");
            cols[2] = new PxColumn("price", "Цена");
            cols[3] = new PxColumn("ma10", "Ma(10)");
            cols[4] = new PxColumn("diff", "Разность %");

            _gr = new RadarRow();
            _rr = new RadarRow();
        }

        public PxColumn[] GetColumns()
        {
            return cols;
        }

        public object[] GetData()
        {
            lock (this)
            {
                _gr.Price = _gazp_lastPrice;
                _rr.Price = _rosn_lastPrice;
            }

            _gr.Ma10 = _gazp_ma10.LastValue != null ? _gazp_ma10.LastValue.Value : 0;
            _rr.Ma10 = _rosn_ma10.LastValue != null ? _rosn_ma10.LastValue.Value : 0;

            _gr.Diff = _gr.Ma10 > 0 ? (_gr.Price - _gr.Ma10) / _gr.Ma10 * 100 : 0;
            _rr.Diff = _rr.Ma10 > 0 ? (_rr.Price - _rr.Ma10) / _rr.Ma10 * 100 : 0;

            return new RadarRow[] { _gr, _rr };
        }

        public void OnLoad()
        {
            _gazp = _platform.GetInstrum("GAZP");
            _gazp_bars = _platform.CreateBarRow(_gazp.InsID, Timeframes.Hour, 100).Result;
            _gazp_ma10 = new Ma(_gazp_bars.Close, AverageMethod.Exponencial, 10);
            _gr.Ticker = _gazp.Ticker;
            _gr.Name = _gazp.ShortName;

            _rosn = _platform.GetInstrum("ROSN");
            _rosn_bars = _platform.CreateBarRow(_rosn.InsID, Timeframes.Hour, 100).Result;
            _rosn_ma10 = new Ma(_rosn_bars.Close, AverageMethod.Exponencial, 10);
            _rr.Ticker = _rosn.Ticker;
            _rr.Name = _rosn.ShortName;

            _platform.AddLog("RadarPlugin", "OnLoad");
            _platform.AddLog("RadarPlugin", "Load GAZP bars:" + _gazp_bars.Dates.Count.ToString());
            _platform.AddLog("RadarPlugin", "Load ROSN bars:" + _rosn_bars.Dates.Count.ToString());

            _platform.Subscribe(_gazp.InsID, OnTick);
            _platform.Subscribe(_rosn.InsID, OnTick);
        }

        private void OnTick(DateTime time, int insId, int lots, decimal price)
        {
            lock (this)
            {
                if (_gazp.InsID == insId)
                {
                    _gazp_lastPrice = price;
                }
                else if (_rosn.InsID == insId)
                {
                    _rosn_lastPrice = price;
                }
            }
        }

        public void OnDestroy()
        {
            _platform.AddLog("RadarPlugin", "OnDestroy");
        }
    }
}
