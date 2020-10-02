using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer.Drawing
{
    public class PriceChart : VisualBase
    {
        private BarRow _bars;
        private ChartBrush _brush;

        public PriceChart(BarRow bars, ChartBrush brush)
        {
            this._bars = bars;
            this._bars.OnChangeBar += bars_OnChangeBar;
            this._brush = brush;
        }

        void bars_OnChangeBar(int index)
        {
            if (Changed != null) Changed(this, new EventArgs());
        }

        public override event VisualChangedEH Changed;

        /// <summary>
        /// Get chart brush
        /// </summary>
        /// <returns></returns>
        public override ChartBrush GetBrush()
        {
            return _brush;
        }

        /// <summary>
        /// The last price (current close price of last bar)
        /// </summary>
        public decimal? LastPrice
        {
            get
            {
                if (_bars == null || _bars.Count <= 0) return null;
                return _bars[_bars.Count - 1].Close;
            }
        }
    }
}
