using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public interface IInstrum
    {
        public int InsID { get; set; }
        public string Ticker { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public int LotSize { get; set; }
        public int Decimals { get; set; }
        public decimal PriceStep { get; set; }
    }
}
