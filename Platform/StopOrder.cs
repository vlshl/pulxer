using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    /// <summary>
    /// Stop order
    /// </summary>
    public class StopOrder
    {
        /// <summary>
        /// Identifier
        /// </summary>
        public int StopOrderID { get; set; }

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
        /// Stop order type (stoploss or takeprofit)
        /// </summary>
        public StopOrderType StopType { get; set; }

        /// <summary>
        /// End time or null (null - infinite)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Activation price (create order)
        /// </summary>
        public decimal AlertPrice { get; set; }

        /// <summary>
        /// Order price or null (null - current market price)
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Order lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Stop order status
        /// </summary>
        public StopOrderStatus Status { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Date and time of activation or completion
        /// </summary>
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// Global StopOrderNo
        /// </summary>
        public long StopOrderNo { get; set; }
    }
}
