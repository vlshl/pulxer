using Platform;
using Pulxer.Drawing;
using System;

namespace Pulxer.Indicators
{
    public class SeriesIndic
    {
        private SeriesChart _chart;
        private Series _series;

        public SeriesIndic(Series s, SeriesRow sr)
        {
            if (s == null || sr == null)
                throw new ArgumentNullException();

            _series = s;

            var penSp = SeriesPropsUtil.Create(_series.Data) as PenSeriesProps;
            ChartBrush brush = new ChartBrush(0, 0, 255);
            if (penSp != null)
            {
                brush = new ChartBrush(penSp.Red, penSp.Green, penSp.Blue, penSp.Alpha);
            }
            _chart = new SeriesChart(sr, brush);
        }

        public Series Series
        {
            get
            {
                return _series;
            }
        }

        /// <summary>
        /// Get visual for drawing
        /// </summary>
        /// <returns></returns>
        public IVisual GetVisual()
        {
            return _chart;
        }
    }
}
