using System;
using System.Collections.Generic;
using System.Text;

namespace Radar
{
    public class RadarRow
    {
        public string Ticker { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal Ma10 { get; set; }
        public decimal Diff { get; set; }
    }
}
