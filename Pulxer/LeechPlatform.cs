﻿using BL;
using Common;
using Common.Data;
using Common.Interfaces;
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
        private Dictionary<int, OnTickDelegate> _insID_onTick = null;

        public LeechPlatform(TickSource tickSrc, IInstrumBL instrumBL, IInsStoreBL insStoreBL, TradeEngine engine, TradeEngineData data, ILogger logger)
        {
            _tickSource = tickSrc;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _engine = engine;
            _data = data;
            _logger = logger;

            _barRows = new List<BarRow>();
            _tickSource.OnTick += _tickSource_OnTick;
            _insID_onTick = new Dictionary<int, OnTickDelegate>();
        }

        private void _tickSource_OnTick(Tick tick)
        {
            if (_insID_onTick.ContainsKey(tick.InsID))
            {
                _insID_onTick[tick.InsID]?.Invoke(tick.Time, tick.Price, tick.Lots);
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
            _logger.AddInfo("Bot:" + source, text);
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
            _tickSource.OnTick -= _tickSource_OnTick;
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

        public int GetHolding(int insID)
        {
            return _engine.GetHoldingLots(insID);
        }

        public void OnTick(int insID, OnTickDelegate onTick)
        {
            if (!_insID_onTick.ContainsKey(insID) && onTick != null)
                _insID_onTick.Add(insID, onTick);
            else if (_insID_onTick.ContainsKey(insID) && onTick == null)
                _insID_onTick.Remove(insID);
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