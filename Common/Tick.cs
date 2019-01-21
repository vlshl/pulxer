using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    /// <summary>
    /// Tick structure
    /// </summary>
    public struct Tick
    {
        /// <summary>
        /// Trade number (from trading system)
        /// </summary>
        public long TradeNo;

        /// <summary>
        /// Trade time
        /// </summary>
        public DateTime Time;

        /// <summary>
        /// Instrument
        /// </summary>
        public int InsID;

        /// <summary>
        /// Quantity
        /// </summary>
        public int Lots;

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price;

        public Tick(long tradeNo, DateTime time, int insID, int lots, decimal price)
        {
            this.TradeNo = tradeNo;
            this.Time = time;
            this.InsID = insID;
            this.Price = price;
            this.Lots = lots;
        }
    }
}
