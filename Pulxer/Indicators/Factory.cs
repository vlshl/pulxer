using Common.Interfaces;
using Pulxer.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Pulxer.Indicators
{
    public interface IFactory
    {
        IndicatorBase CreateIndicator(string key);
        IEnumerable<FactoryItem> GetIndicators();
    }

    /// <summary>
    /// Indicator factory
    /// </summary>
    public class Factory : IFactory
    {
        private Dictionary<string, string> _indicKey_name = null;
        private IValueRowSourcesProvider _vrSrcProv = null;
        private IDependencyManager _depManager;

        public Factory(IValueRowSourcesProvider vrSrcProv, IDependencyManager depManager)
        {
            _vrSrcProv = vrSrcProv;
            _depManager = depManager;

            _indicKey_name = new Dictionary<string, string>
            {
                { "Ma", "Moving Average" },
                { "Ama", "Adaptive Moving Average" },
                //{"Macd", "Macd"},
                { "Bb", "Bollinger Bands" }
            };
        }

        /// <summary>
        /// Get all available indicators (pairs [key, name])
        /// </summary>
        /// <returns></returns>
        public IEnumerable<FactoryItem> GetIndicators()
        {
            return _indicKey_name.Select(r => new FactoryItem(r.Key, r.Value)).ToList();
        }

        /// <summary>
        /// Create indicator by key
        /// </summary>
        /// <param name="key">Key (Ma - moving average, Bb - bollinger bands etc.)</param>
        /// <returns></returns>
        public IndicatorBase CreateIndicator(string key)
        {
            if (_vrSrcProv == null) return null;

            IndicatorBase indic = null;
            switch (key)
            {
                case "Ma":
                    indic = new MaIndicator(_vrSrcProv, _indicKey_name[key], _depManager);
                    break;
                case "Ama":
                    indic = new AmaIndicator(_vrSrcProv, _indicKey_name[key], _depManager);
                    break;
                case "Bb":
                    indic = new BbIndicator(_vrSrcProv, _indicKey_name[key], _depManager);
                    break;
            }

            return indic;
        }
    }
}
