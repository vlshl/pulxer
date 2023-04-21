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
        public int Decimals { get; set; }
        public decimal Ma1 { get; set; }
        public decimal Diff1 { get; set; }
        public decimal Ma2 { get; set; }
        public decimal Diff2 { get; set; }
        public decimal Ma3 { get; set; }
        public decimal Diff3 { get; set; }
    }
}
