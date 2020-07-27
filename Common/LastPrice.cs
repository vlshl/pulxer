using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public struct LastPrice
    {
        public string Ticker { get; set; }
        public int Date { get; set; }
        public int Time { get; set; }
        public decimal Price { get; set; }
        public int Lots { get; set; }

        public LastPrice(string ticker, DateTime time, decimal price, int lots)
        {
            Ticker = ticker;
            Date = time.Year * 10000 + time.Month * 100 + time.Day;
            Time = time.Hour * 10000 + time.Minute * 100 + time.Second;
            Price = price;
            Lots = lots;
        }
    }
}
