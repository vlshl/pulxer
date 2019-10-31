using Indic;
using Platform;
using System;
using System.Threading.Tasks;

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
            _platform.OnTick(_gazp.InsID, OnTick, false);
            _platform.AddLog("MyBot", "Closed");
        }

        public async override Task<IBotResult> Initialize(IBotParams botParams)
        {
            decimal summa = _platform.GetCurrentSumma();

            _platform.AddLog("MyBot", "Initialize ..." + summa.ToString());
            _gazp = _platform.GetInstrum("GAZP");
            if (_gazp == null) return _platform.BotError("Не найдет тикер");

            _bars1 = await _platform.CreateBarRow(_gazp.InsID, Timeframes.Min5, 5);
            if (_bars1 == null)
            {
                _platform.AddLog("MyBot", "Не создан BarRow");
                return _platform.BotError("Не создан BarRow");
            }
            _bars2 = await _platform.CreateBarRow(_gazp.InsID, Timeframes.Hour, 5);
            if (_bars2 == null)
            {
                _platform.AddLog("MyBot", "Не создан BarRow");
                return _platform.BotError("Не создан BarRow");
            }

            _bars1_ma = new Ma(_bars1.Close, AverageMethod.Exponencial, 10);
            _bars1.OnCloseBar += Bars1_OnCloseBar;
            _bars1_ma.Change += _bars1_ma_Change;

            _bars2_ma = new Ma(_bars2.Close, AverageMethod.Exponencial, 10);
            _bars2.OnCloseBar += Bars2_OnCloseBar;

            _platform.OnTick(_gazp.InsID, OnTick, true);

            _platform.AddLog("MyBot", "Initialized");

            return _platform.BotSuccess();
        }

        private void _bars1_ma_Change(ValueRow vr, bool isReset)
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

        private void OnTick(DateTime time, decimal price, int lots)
        {
            if (ma1 == 0) return;
            if (_order != null && _order.Status == OrderStatus.Active) return; // находимся в состоянии активной заявки

            int t = time.Hour * 10000 + time.Minute * 100 + time.Second;
            int hold = _platform.GetHoldingLots(_gazp.InsID);
            
            // вход
            if (hold == 0 && price >= ma1 + 0.2m && t >= 103000 && t < 183000)
            {
                int openLots = (int)decimal.Round(_platform.GetCurrentSumma() / (price * 10)) - 1;
                if (openLots > 0)
                {
                    _order = _platform.AddBuyOrder(_gazp.InsID, null, openLots);
                }
            }

            // выход
            if (hold > 0 && (price <= ma1 || t >= 183000))
            {
                _order = _platform.AddSellOrder(_gazp.InsID, null, hold);
            }
        }

        private Order _order = null;
    }
}
