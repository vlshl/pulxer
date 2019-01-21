using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    /// <summary>
    /// History bar data
    /// </summary>
    public class DbBarHistory
    {
        public DbBarHistory()
        {
            this.InsStoreID = 0;
            this.Time = 0;
            this.OpenPrice = 0;
            this.CloseDelta = 0;
            this.HighDelta = 0;
            this.LowDelta = 0;
            this.Volume = 0;
        }

        /// <summary>
        /// InsStore reference
        /// </summary>
        public int InsStoreID { get; set; }

        /// <summary>
        /// Date and time of bar (accurate to a second)
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Open bar price
        /// </summary>
        public int OpenPrice { get; set; }

        /// <summary>
        /// Close bar price delta
        /// </summary>
        public short CloseDelta { get; set; }

        /// <summary>
        /// High bar price delta
        /// </summary>
        public short HighDelta { get; set; }

        /// <summary>
        /// Low bar price delta
        /// </summary>
        public short LowDelta { get; set; }

        /// <summary>
        /// Total bar volume (in pieces, not lots)
        /// </summary>
        public int Volume { get; set; }
    }
}
