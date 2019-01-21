using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    /// <summary>
    /// Trade order
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Identifier
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
        /// Buy or sell order
        /// </summary>
        public BuySell BuySell { get; set; }

        /// <summary>
        /// Lots count
        /// </summary>
        public int LotCount { get; set; }

        /// <summary>
        /// Price or null (null - current market price)
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary>
        /// Order status
        /// </summary>
        public OrderStatus Status { get; set; }

        /// <summary>
        /// Account reference
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Stop order reference or null
        /// </summary>
        public int? StopOrderID { get; set; }

        /// <summary>
        /// Global OrderNo
        /// </summary>
        public long OrderNo { get; set; }
    }
}
