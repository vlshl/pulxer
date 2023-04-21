using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer.Plugin
{
    public class PluginPlatform : IPluginPlatform
    {
        private readonly ILogger<PluginPlatform> _logger;
        private readonly IInstrumBL _instrumBL;
        private readonly TickDispatcher _tickDispatcher;
        private readonly IInsStoreBL _insStoreBL;
        private List<BarRow> _barRows;
        private Dictionary<int, OnPxTick> _ins_onTick;

        public PluginPlatform(ILogger<PluginPlatform> logger, IInstrumBL instrumBL, TickDispatcher tickDispatcher, IInsStoreBL insStoreBL)
        {
            _logger = logger;
            _instrumBL = instrumBL;
            _tickDispatcher = tickDispatcher;
            _insStoreBL = insStoreBL;
            _barRows = new List<BarRow>();
            _ins_onTick = new Dictionary<int, OnPxTick>();
        }

        public void AddLog(string source, string text)
        {
            _logger.LogInformation(source + "\t" + text);
        }

        public async Task<IBarRow> CreateBarRow(int insID, Timeframes tf, int historyDays)
        {
            BarRow bars = new BarRow(tf, insID);

            var insStore = _insStoreBL.GetLoadHistoryInsStore(insID, tf); // наиболее подходящий insStore
            if (insStore == null) return null;

            var endHistoryDate = _tickDispatcher.SessionDate.AddDays(-1);
            var startHistoryDate = _insStoreBL.GetStartHistoryDate(insStore.InsStoreID, endHistoryDate, historyDays);
            if (startHistoryDate != null)
            {
                await _insStoreBL.LoadHistoryAsync(bars, insID, startHistoryDate.Value, endHistoryDate, insStore.InsStoreID);
                bars.TickDispatcher = _tickDispatcher;
                _logger?.LogInformation(string.Format("LoadHistory bars count={0}, insId={1}, tf={2}, start={3}, end={4}", 
                    bars.Count.ToString(), 
                    insID.ToString(), 
                    tf.ToString(), 
                    startHistoryDate.Value.ToString("yyyy-MM-dd HH:mm:ss"), 
                    endHistoryDate.ToString("yyyy-MM-dd HH:mm:ss")));

            }
            else
            {
                _logger?.LogWarning("Bar history not loaded and tick dispatcher not attached, startHistoryDate is null.");
            }
            _barRows.Add(bars);

            return bars;
        }

        public IInstrum GetInstrum(string ticker)
        {
            return _instrumBL.GetInstrum(ticker);
        }

        public void Close()
        {
            foreach (var br in _barRows) br.CloseBarRow();
            _barRows.Clear();
            foreach(var insId in _ins_onTick.Keys) _tickDispatcher.Unsubscribe(this, insId);
            _ins_onTick.Clear();
        }

        public void Subscribe(int insId, OnPxTick onTick)
        {
            if (_ins_onTick.ContainsKey(insId)) return;

            _tickDispatcher.Subscribe(this, insId, OnTick);
            _ins_onTick.Add(insId, onTick);
        }

        private void OnTick(Tick tick)
        {
            if (!_ins_onTick.ContainsKey(tick.InsID)) return;
            _ins_onTick[tick.InsID](tick.Time, tick.InsID, tick.Lots, tick.Price);
        }

        public DateTime GetSessionDate()
        {
            return _tickDispatcher.SessionDate;
        }
    }
}




