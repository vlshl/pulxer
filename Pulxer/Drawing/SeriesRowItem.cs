using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer.Drawing
{
    public class SeriesRowItem
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? EndValue { get; set; }
        public string Data { get; set; }
        public PenSeriesProps PenSp { get; set; }
}
}
