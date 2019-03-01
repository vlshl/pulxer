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
        private IBarRow _bars1;
        private Ma _bars1_ma;
        private IBarRow _bars2;
        private Ma _bars2_ma;
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

            _bars1 = await _platform.CreateBarRow(_gazp.InsID, Timeframes.Min5, 5);
            if (_bars1 == null)
            {
                _platform.AddLog("MyBot", "Не создан BarRow");
                return;
            }
            _bars2 = await _platform.CreateBarRow(_gazp.InsID, Timeframes.Hour, 5);
            if (_bars2 == null)
            {
                _platform.AddLog("MyBot", "Не создан BarRow");
                return;
            }

            _bars1_ma = new Ma(_bars1.Close, AverageMethod.Exponencial, 10);
            _bars1.OnCloseBar += Bars1_OnCloseBar;
            _bars1_ma.Change += _bars1_ma_Change;

            _bars2_ma = new Ma(_bars2.Close, AverageMethod.Exponencial, 10);
            _bars2.OnCloseBar += Bars2_OnCloseBar;

            _platform.OnTick(_gazp.InsID, OnTick);


            //_platform.OnTimer(OnTimer);

            _platform.AddLog("MyBot", "Initialized");
        }

        private void _bars1_ma_Change(bool isReset)
        {
            if (!isReset && _bars1_ma.LastValue != null)
            {
                ma1 = _bars1_ma.LastValue.Value;
            }
            else
            {
                ma1 = 0;
            }
        }
        private decimal ma1 = 0;

        private void Bars1_OnCloseBar(int index)
        {
            var time = _bars1[index].Time;
            decimal? close = _bars1.Close[index];
        }

        private void Bars2_OnCloseBar(int index)
        {
            var time = _bars2[index].Time;
            decimal? close = _bars2.Close[index];
        }

        //private void OnTimer(DateTime? time, int delay)
        //{
        //    _platform.AddLog("MyBot", string.Format("OnTimer: time={0}, delay={1}",
        //        (time != null ? time.Value.ToString("dd.MM.yyyy HH:mm:ss") : "null"), delay.ToString()));
        //}

        private void OnTick(DateTime time, decimal price, int lots)
        {
            if (ma1 == 0) return;
            int t = time.Hour * 10000 + time.Minute * 100 + time.Second;

            int hold = _platform.GetHolding(_gazp.InsID);

            if (_isOpeningPos && hold > 0) _isOpeningPos = false;
            if (_isClosingPos && hold == 0) _isClosingPos = false;

            
            // вход
            if (!_isOpeningPos && !_isClosingPos && hold == 0 && price >= ma1 + 0.5m && t >= 103000 && t < 183000)
            {
                int openLots = (int)decimal.Round(_platform.GetCurrentSumma() / (price * 10));

                _platform.AddBuyOrder(_gazp.InsID, null, openLots);
                _isOpeningPos = true;
            }

            // выход
            if (!_isOpeningPos && !_isClosingPos && hold > 0 && (price <= ma1 || t >= 183000))
            {
                _platform.AddSellOrder(_gazp.InsID, null, hold);
                _isClosingPos = true;
            }

            if (t >= 183500 && (_isOpeningPos || _isClosingPos))
            {
                _isOpeningPos = _isClosingPos = false;
            }
        }

        private bool _isOpeningPos;
        private bool _isClosingPos;
    }
}
