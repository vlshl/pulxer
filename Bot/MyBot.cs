using Indic;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bot
{
    public class MyBot : BotBase
    {
        private readonly ILeechPlatform _platform;
        private IBarRow _bars;
        private Ma _ma;
        private IInstrum _gazp = null;

        public MyBot(ILeechPlatform platform)
        {
            _platform = platform;
        }

        public override void Close()
        {
            _platform.OnTimer(null);
            _platform.OnTick(_gazp.InsID, null);
            _platform.AddLog("MyBot", "Closed");
        }

        public async override void Initialize(string data)
        {
            _platform.AddLog("MyBot", "Initialize ...");
            _gazp = _platform.GetInstrum("GAZP");
            if (_gazp == null) return;

            _bars = await _platform.CreateBarRow(_gazp.InsID, Timeframes.Min, 5);
            if (_bars == null)
            {
                _platform.AddLog("MyBot", "Не создан BarRow");
                return;
            }

            _ma = new Ma(_bars.Close, AverageMethod.Exponencial, 10);

            _bars.OnCloseBar += Bars_OnCloseBar;
            _bars.Close.Change += Close_Change;

            _platform.OnTimer(OnTimer);
            _platform.OnTick(_gazp.InsID, OnTick);

            _platform.AddLog("MyBot", "Initialized");
        }

        private void Close_Change(bool isReset)
        {
            //_platform.AddLog("MyBot", "CloseChange, isReset=" + isReset.ToString());
        }

        private void Bars_OnCloseBar(int index)
        {
            var time = _bars[index].Time;
            decimal? close = _bars.Close[index];

            //_platform.AddLog("MyBot", string.Format("CloseBar: time={0}, close={1}, index={2}",
            //    time.ToString("HH:mm:ss"), (close != null ? close.Value.ToString() : "null"), index.ToString()));
        }

        private void OnTimer(DateTime? time, int delay)
        {
            //_platform.AddLog("MyBot", string.Format("OnTimer: time={0}, delay={1}",
            //    (time != null ? time.Value.ToString("dd.MM.yyyy HH:mm:ss") : "null"), delay.ToString()));
        }

        private void OnTick(DateTime time, decimal price, int lots)
        {
            //_platform.AddLog("MyBot", string.Format("OnTick: time={0}, price={1}, lots={2}",
            //    time.ToString("dd.MM.yyyy HH:mm:ss"), price.ToString(), lots.ToString()));
        }
    }
}
