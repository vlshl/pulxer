using Common;
using System.Linq;

namespace Pulxer.Drawing
{
    public class SeriesChart : VisualBase
    {
        private SeriesRow _row;
        private ChartBrush _brush;

        public SeriesChart(SeriesRow row, ChartBrush brush)
        {
            _row = row;
            _brush = brush;
        }
    }
}
