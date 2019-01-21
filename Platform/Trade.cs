using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    /// <summary>
    /// Trade
    /// </summary>
    public class Trade
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int TradeID { get; set; }

        /// <summary>
        /// Order reference (not null)
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// Create date and time
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Fin. instrument reference
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Buy or sell
        /// </summary>
        public BuySell BuySell { get; set; }

        /// <summary>
        /// Lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Trade price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Broker commission
        /// </summary>
        public decimal Comm { get; set; }

        /// <summary>
        /// Trade number (from external trading system)
        /// </summary>
        public long TradeNo { get; set; }
    }
}
