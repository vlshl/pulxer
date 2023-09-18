using Common;
using Common.Interfaces;
using Indic;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Radar
{
    public class RadarPlugin : IPxPlugin, ITgContext
    {
        private PxColumn[] _cols;
        private readonly IPluginPlatform _platform;
        private Dictionary<int, RadarRow> _insid_rr;
        private Dictionary<int, IBarRow> _insid_bars1;
        private Dictionary<int, IBarRow> _insid_bars2;
        private Dictionary<int, IBarRow> _insid_bars3;
        private Dictionary<int, Ma> _insid_ma1;
        private Dictionary<int, Ma> _insid_ma2;
        private Dictionary<int, Ma> _insid_ma3;
        private Dictionary<int, decimal> _insid_price;
        private string _pluginPath;
        private int _maDecimalsAdd;
        private int _diffDecimals;
        private string _redColor;
        private string _greenColor;
        private readonly ITgContextManager _tgContextManager;
        //private ITgContext _tgContext;

        public RadarPlugin(IPluginPlatform platform, string pluginPath, ITgContextManager tgContextManager)
        {
            _platform = platform;
            _pluginPath = pluginPath;
            _maDecimalsAdd = 0;
            _diffDecimals = 0;
            _redColor = _greenColor = "";

            _insid_rr = new Dictionary<int, RadarRow>();
            _insid_bars1 = new Dictionary<int, IBarRow>();
            _insid_bars2 = new Dictionary<int, IBarRow>();
            _insid_bars3 = new Dictionary<int, IBarRow>();
            _insid_ma1 = new Dictionary<int, Ma>();
            _insid_ma2 = new Dictionary<int, Ma>();
            _insid_ma3 = new Dictionary<int, Ma>();
            _insid_price = new Dictionary<int, decimal>();

            _cols = new PxColumn[12];
            _cols[0] = new PxColumn("ticker", "Тикер");
            _cols[1] = new PxColumn("name", "Инструмент");
            _cols[2] = new PxColumn("price", "Цена");
            _cols[3] = new PxColumn("ma1", "Ma1");
            _cols[4] = new PxColumn("ma2", "Ma2");
            _cols[5] = new PxColumn("ma3", "Ma3");
            _cols[6] = new PxColumn("diff1", "Diff1");
            _cols[7] = new PxColumn("diff2", "Diff2");
            _cols[8] = new PxColumn("diff3", "Diff3");
            _cols[9] = new PxColumn("diff12", "Diff12");
            _cols[10] = new PxColumn("diff13", "Diff13");
            _cols[11] = new PxColumn("diff23", "Diff23");
            _tgContextManager = tgContextManager;
        }

        ~RadarPlugin()
        {
            _platform.AddLog("radar", "RadarPlugin destructor");
        }

        public PxColumn[] GetColumns()
        {
            return _cols;
        }

        public object[] GetData()
        {
            foreach (int insid in _insid_rr.Keys)
            {
                var rr = _insid_rr[insid];
                lock (_insid_price)
                {
                    rr.Price = _insid_price[insid];
                }
                var ma1Last = _insid_ma1[insid].LastValue;
                var ma2Last = _insid_ma2[insid].LastValue;
                var ma3Last = _insid_ma3[insid].LastValue;

                decimal ma1 = ma1Last != null ? ma1Last.Value : 0;
                decimal diff1 = rr.Ma1 > 0 ? (rr.Price - rr.Ma1) / rr.Ma1 * 100 : 0;
                rr.Ma1 = MaRound(ma1, rr.Decimals);
                rr.Diff1 = DiffRound(diff1);

                decimal ma2 = ma2Last != null ? ma2Last.Value : 0;
                decimal diff2 = rr.Ma2 > 0 ? (rr.Price - rr.Ma2) / rr.Ma2 * 100 : 0;
                rr.Ma2 = MaRound(ma2, rr.Decimals);
                rr.Diff2 = DiffRound(diff2);

                decimal ma3 = ma3Last != null ? ma3Last.Value : 0;
                decimal diff3 = rr.Ma3 > 0 ? (rr.Price - rr.Ma3) / rr.Ma3 * 100 : 0;
                rr.Ma3 = MaRound(ma3, rr.Decimals);
                rr.Diff3 = DiffRound(diff3);

                decimal diff12 = ma2 > 0 ? (ma1 - ma2) / ma2 * 100 : 0;
                decimal diff13 = ma3 > 0 ? (ma1 - ma3) / ma3 * 100 : 0;
                decimal diff23 = ma3 > 0 ? (ma2 - ma3) / ma3 * 100 : 0;

                rr.Diff12 = DiffRound(diff12);
                rr.Diff13 = DiffRound(diff13);
                rr.Diff23 = DiffRound(diff23);

                Dictionary<string, PxCellStyle> styles = new Dictionary<string, PxCellStyle>();
                var s1 = new PxCellStyle(); 
                s1.BackColor = rr.Diff1 < 0 ? _redColor : _greenColor; 
                styles.Add("diff1", s1);
                
                var s2 = new PxCellStyle(); 
                s2.BackColor = rr.Diff2 < 0 ? _redColor : _greenColor; 
                styles.Add("diff2", s2);
                
                var s3 = new PxCellStyle(); 
                s3.BackColor = rr.Diff3 < 0 ? _redColor : _greenColor; 
                styles.Add("diff3", s3);

                var s12 = new PxCellStyle();
                s12.BackColor = rr.Diff12 < 0 ? _redColor : _greenColor;
                styles.Add("diff12", s12);

                var s13 = new PxCellStyle();
                s13.BackColor = rr.Diff13 < 0 ? _redColor : _greenColor;
                styles.Add("diff13", s13);

                var s23 = new PxCellStyle();
                s23.BackColor = rr.Diff23 < 0 ? _redColor : _greenColor;
                styles.Add("diff23", s23);

                rr.Styles = styles;
            }

            return _insid_rr.Values.ToArray();
        }

        private decimal MaRound(decimal ma, int decimals)
        {
            return decimal.Round(ma, decimals + _maDecimalsAdd, MidpointRounding.AwayFromZero);
        }

        private decimal DiffRound(decimal d)
        {
            return decimal.Round(d, _diffDecimals, MidpointRounding.AwayFromZero);
        }

        public void OnLoad()
        {
            Config c = new Config(_platform, _pluginPath);
            bool isSuccess = c.Load();
            if (!isSuccess)
            {
                _platform.AddLog("radar", "Load config error");
                return;
            }
            RadarConf conf = c.RadarConfig;

            _cols[3].Name = conf.Ma1.Name;
            _cols[4].Name = conf.Ma2.Name;
            _cols[5].Name = conf.Ma3.Name;
            _cols[6].Name = conf.Ma1.DiffName;
            _cols[7].Name = conf.Ma2.DiffName;
            _cols[8].Name = conf.Ma3.DiffName;
            _cols[9].Name = conf.Diff12Name;
            _cols[10].Name = conf.Diff13Name;
            _cols[11].Name = conf.Diff23Name;

            _maDecimalsAdd = conf.MaDecimalsAdd;
            _diffDecimals = conf.DiffDecimals;
            _redColor = conf.RedColor;
            _greenColor = conf.GreenColor;

            _insid_rr.Clear();
            _insid_bars1.Clear(); _insid_bars2.Clear(); _insid_bars3.Clear();
            _insid_ma1.Clear(); _insid_ma2.Clear(); _insid_ma3.Clear();
            _insid_price.Clear();

            foreach (var ticker in conf.Tickers)
            {
                var instrum = _platform.GetInstrum(ticker);
                if (instrum == null)
                {
                    _platform.AddLog("radar", "Ticker not found: " + ticker);
                    continue;
                }

                RadarRow rr = new RadarRow();
                rr.Ticker = ticker;
                rr.Name = instrum.ShortName;
                rr.Decimals = instrum.Decimals;

                var bars1 = _platform.CreateBarRow(instrum.InsID, conf.Ma1.Timeframe, conf.Ma1.HistoryDays).Result;
                _insid_bars1.Add(instrum.InsID, bars1);
                var ma1 = new Ma(bars1.Close, conf.Ma1.Average, conf.Ma1.N);
                _insid_ma1.Add(instrum.InsID, ma1);

                var bars2 = _platform.CreateBarRow(instrum.InsID, conf.Ma2.Timeframe, conf.Ma2.HistoryDays).Result;
                _insid_bars2.Add(instrum.InsID, bars2);
                var ma2 = new Ma(bars2.Close, conf.Ma2.Average, conf.Ma2.N);
                _insid_ma2.Add(instrum.InsID, ma2);

                var bars3 = _platform.CreateBarRow(instrum.InsID, conf.Ma3.Timeframe, conf.Ma3.HistoryDays).Result;
                _insid_bars3.Add(instrum.InsID, bars3);
                var ma3 = new Ma(bars3.Close, conf.Ma3.Average, conf.Ma3.N);
                _insid_ma3.Add(instrum.InsID, ma3);

                _insid_price.Add(instrum.InsID, 0);
                _platform.Subscribe(instrum.InsID, OnTick);

                _insid_rr.Add(instrum.InsID, rr);
                _platform.AddLog("radar", "Ticker load: " + ticker);
            }

            var isRegSuccess = _tgContextManager.RegisterContext(this);
            if (isRegSuccess)
            {
                _platform.AddLog("radar", "Context registered successfully");
            }
            else
            {
                _platform.AddLog("radar", "Context register error");
            }
        }

        private void OnTick(DateTime time, int insId, int lots, decimal price)
        {
            lock (_insid_price)
            {
                if (_insid_price.ContainsKey(insId))
                {
                    _insid_price[insId] = price;
                }
            }
        }

        public void OnDestroy()
        {
            _tgContextManager.UnregisterContext(this);
            _platform.AddLog("radar", "OnDestroy");
        }

        #region ITgContext
        public string GetTgName()
        {
            return "Radar";
        }

        public string GetTgCommand()
        {
            return "/radar";
        }

        public void OnSetTgContext()
        {
        }

        public void OnCommand(string cmd)
        {
        }

        public void OnMessage(string msg)
        {
        }
        #endregion
    }
}
