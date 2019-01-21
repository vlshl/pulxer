using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    /// <summary>
    /// Price bar
    /// </summary>
    public class Bar
    {
        /// <summary>
        /// Start time of bar
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Start time of the next bar
        /// </summary>
        public DateTime NextBarTime { get; set; }

        /// <summary>
        /// Open price
        /// </summary>
        public decimal Open { get; set; }

        /// <summary>
        /// Close price
        /// </summary>
        public decimal Close { get; set; }

        /// <summary>
        /// High price
        /// </summary>
        public decimal High { get; set; }

        /// <summary>
        /// Low price
        /// </summary>
        public decimal Low { get; set; }

        /// <summary>
        /// Total volume
        /// </summary>
        public long Volume { get; set; }

        public Bar()
        {
            this.Time = DateTime.MinValue;
        }

        public Bar(DateTime time, DateTime nextBarTime, decimal open, decimal close,
            decimal high, decimal low, long volume = 0)
        {
            this.Time = time; this.NextBarTime = nextBarTime;
            this.Open = open; this.Close = close;
            this.High = high; this.Low = low;
            this.Volume = volume;
        }

        public Bar(DateTime time, Timeframes tf)
            : this()
        {
            var dates = TfHelper.GetDates(time, tf);
            this.Time = dates[0];
            this.NextBarTime = dates[1];
        }
    }
}
