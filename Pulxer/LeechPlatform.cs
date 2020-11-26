using BL;
using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer
{
    public class LeechPlatform : ILeechPlatform
    {
        private readonly TickSource _tickSource = null;
        private readonly IInstrumBL _instrumBL = null;
        private readonly IInsStoreBL _insStoreBL = null;
        private List<BarRow> _barRows;
        private OnTimerDelegate _onTimer = null;
        private DateTime? _nextSecTime = null;
        //private Task _onTimerTask = null;
        private ILogger _logger = null;
        private readonly TradeEngine _engine = null;
        private readonly TradeEngineData _data;
        private readonly SeriesData _seriesData;
        private Dictionary<int, List<OnTickDelegate>> _insID_onTicks = null;
        private Dictionary<int, IPosManager> _insID_pm = null;

        public LeechPlatform(TickSource tickSrc, IInstrumBL instrumBL, IInsStoreBL insStoreBL, TradeEngine engine, 
            TradeEngineData data, SeriesData seriesData, ILogger logger)
        {
            _tickSource = tickSrc;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _engine = engine;
            _data = data;
            _seriesData = seriesData;
            _logger = logger;

            _barRows = new List<BarRow>();
            _tickSource.OnTick += _tickSource_OnTick;
            _insID_onTicks = new Dictionary<int, List<OnTickDelegate>>();
            _insID_pm = new Dictionary<int, IPosManager>();
        }

        private void _tickSource_OnTick(Tick tick)
        {
            if (_insID_onTicks.ContainsKey(tick.InsID))
            {
                var onTicks = _insID_onTicks[tick.InsID];
                foreach (var onTick in onTicks) { onTick?.Invoke(tick.Time, tick.Price, tick.Lots); }
            }

            if (_onTimer != null)
            {
                if (_nextSecTime == null || tick.Time >= _nextSecTime.Value)
                {
                    _onTimer(tick.Time, 0);
                    _nextSecTime = tick.Time.AddSeconds(1);
                }
            }
        }

        public void AddLog(string source, string text)
        {
            _logger.LogInformation("Bot:" + source + " " + text);
        }

        public IInstrum GetInstrum(string ticker)
        {
            return _instrumBL.GetInstrum(ticker);
        }

        public async Task<IBarRow> CreateBarRow(int insID, Timeframes tf, int historyDays)
        {
            BarRow bars = new BarRow(tf, _tickSource, insID);

            var insStore = _insStoreBL.GetLoadHistoryInsStore(insID, tf); // наиболее подходящий insStore
            if (insStore == null) return null;

            DateTime? curDate = _tickSource.CurrentTime;
            if (curDate == null) return null;

            var endHistoryDate = curDate.Value.AddDays(-1);
            var startHistoryDate = _insStoreBL.GetStartHistoryDate(insStore.InsStoreID, endHistoryDate, historyDays);
            if (startHistoryDate != null)
            {
                await _insStoreBL.LoadHistoryAsync(bars, insID, startHistoryDate.Value, endHistoryDate, insStore.InsStoreID);
            }
            _barRows.Add(bars);

            return bars;
        }

        public void Close()
        {
            foreach (var br in _barRows) br.CloseBarRow();
            _barRows.Clear();

            _tickSource.OnTick -= _tickSource_OnTick;

            foreach (var insID in _insID_pm.Keys) _insID_pm[insID].ClosePosManager();
            _insID_pm.Clear();
        }

        public void OnTimer(OnTimerDelegate onTimer)
        {
            _onTimer = onTimer;
        }

        public Order AddBuyOrder(int insID, decimal? price, int lots)
        {
            return _engine.AddOrder(insID, BuySell.Buy, price, lots);
        }

        public Order AddSellOrder(int insID, decimal? price, int lots)
        {
            return _engine.AddOrder(insID, BuySell.Sell, price, lots);
        }

        public void RemoveOrder(Order order)
        {
            _engine.RemoveOrder(order);
        }

        public Order ModifyOrder(Order order)
        {
            return _engine.ModifyOrder(order);
        }

        public StopOrder LongStopLoss(int insID, decimal alertPrice, decimal? price, int lots)
        {
            return _engine.AddStopOrder(insID, BuySell.Sell, StopOrderType.StopLoss, null, alertPrice, price, lots);
        }

        public StopOrder LongTakeProfit(int insID, decimal alertPrice, decimal? price, int lots)
        {
            return _engine.AddStopOrder(insID, BuySell.Sell, StopOrderType.TakeProfit, null, alertPrice, price, lots);
        }

        public StopOrder ShortStopLoss(int insID, decimal alertPrice, decimal? price, int lots)
        {
            return _engine.AddStopOrder(insID, BuySell.Buy, StopOrderType.StopLoss, null, alertPrice, price, lots);
        }

        public StopOrder ShortTakeProfit(int insID, decimal alertPrice, decimal? price, int lots)
        {
            return _engine.AddStopOrder(insID, BuySell.Buy, StopOrderType.TakeProfit, null, alertPrice, price, lots);
        }

        public void RemoveStopOrder(StopOrder so)
        {
            _engine.RemoveStopOrder(so);
        }

        public IEnumerable<StopOrder> GetStopOrders(int insID)
        {
            return _engine.GetActiveStopOrders(insID).ToList();
        }

        public void RemoveStopOrders(int insID)
        {
            _engine.RemoveActiveStopOrders(insID);
        }

        public int GetHoldingLots(int insID)
        {
            return _engine.GetHoldingLots(insID);
        }

        public void OnTick(int insID, OnTickDelegate onTick, bool isSubscribe)
        {
            if (onTick == null) return;

            if (isSubscribe)
            {
                if (!_insID_onTicks.ContainsKey(insID))
                {
                    _insID_onTicks.Add(insID, new List<OnTickDelegate>() { onTick });
                }
                else
                {
                    var onTicks = _insID_onTicks[insID];
                    if (!onTicks.Contains(onTick)) { onTicks.Add(onTick); }
                }
            }
            else
            {
                if (_insID_onTicks.ContainsKey(insID))
                {
                    var onTicks = _insID_onTicks[insID];
                    if (onTicks.Contains(onTick))
                    {
                        onTicks.Remove(onTick);
                        if (!onTicks.Any())
                        {
                            _insID_onTicks.Remove(insID);
                        }
                    }
                }
            }
        }

        public decimal GetCommPerc()
        {
            if (_data == null) return 0;
            var account = _data.GetAccount();
            if (account == null) return 0;

            return account.CommPerc;
        }

        public bool GetShortEnable()
        {
            if (_data == null) return false;
            var account = _data.GetAccount();
            if (account == null) return false;

            return account.IsShortEnable;
        }

        public decimal GetInitialSumma()
        {
            if (_data == null) return 0;
            var cash = _data.GetCash();
            if (cash == null) return 0;

            return cash.Initial;
        }

        public decimal GetCurrentSumma()
        {
            if (_data == null) return 0;
            var cash = _data.GetCash();
            if (cash == null) return 0;

            return cash.Current;
        }

        public IPosManager GetPosManager(int insID)
        {
            if (_insID_pm.ContainsKey(insID))
            {
                return _insID_pm[insID];
            }
            else
            {
                var pm = new PosManager(this, insID);
                _insID_pm.Add(insID, pm);

                return pm;
            }
        }

        #region Series
        public int OpenSeries(string key, string name, SeriesAxis axis, ISeriesProps sp = null)
        {
            return _seriesData.OpenSeries(key, name, axis, sp);
        }

        public bool AddSeriesValue(int seriesID, DateTime time, decimal val, ISeriesProps sp = null)
        {
            return _seriesData.AddSeriesValue(seriesID, time, val, sp);
        }

        public void SubscribeValueRow(int seriesID, ValueRow valueRow, Timeline timeline, Func<decimal, ISeriesProps> funcSp = null)
        {
            _seriesData.SubscribeValueRow(seriesID, valueRow, timeline, funcSp);
        }

        public IBotResult BotSuccess(string msg = "")
        {
            return new BotResult(true, msg);
        }

        public IBotResult BotError(string msg)
        {
            return new BotResult(false, msg);
        }
        #endregion

        //public void OnTimer(OnTimerDelegate onTimer)
        //{
        //    _onTimer = onTimer;
        //    if (_onTimer != null && _onTimerTask == null)
        //    {
        //        _onTimerTask = Task.Factory.StartNew(() => 
        //        {
        //            while (_onTimer != null)
        //            {
        //                var now = DateTime.Now;
        //                DateTime nextSec = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second).AddSeconds(1);
        //                long waitToNextSec = nextSec.Ticks - now.Ticks;
        //                if (waitToNextSec > 0)
        //                {
        //                    Thread.Sleep(new TimeSpan(waitToNextSec));
        //                }

        //                if (_lastTickTime == null)
        //                {
        //                    _onTimer(null, 0);
        //                }
        //                else
        //                {
        //                    int delay = (int)(new TimeSpan(nextSec.Ticks - _lastTickTime.Value.Ticks).TotalMilliseconds);
        //                    _onTimer(_lastTickTime.Value, delay);
        //                }


        //            }
        //        });
        //    }
        //    else if (_onTimer == null && _onTimerTask != null)
        //    {
        //        _onTimerTask = null;
        //    }
        //}
    }
}
