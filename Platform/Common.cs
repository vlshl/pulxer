using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    /// <summary>
    /// Buy or Sell
    /// </summary>
    public enum BuySell : byte
    {
        Buy = 0,
        Sell = 1
    }

    /// <summary>
    /// Stop order type - Take profit or stop loss
    /// </summary>
    public enum StopOrderType : byte
    {
        TakeProfit = 0,
        StopLoss = 1
    }

    /// <summary>
    /// Stop order status
    /// </summary>
    public enum StopOrderStatus : byte
    {
        /// <summary>
        /// Active
        /// </summary>
        Active = 0,

        /// <summary>
        /// Executed (new order)
        /// </summary>
        Order = 1,

        /// <summary>
        /// Remove by user
        /// </summary>
        Remove = 2,

        /// <summary>
        /// End of time
        /// </summary>
        EndTime = 3,

        /// <summary>
        /// Reject by system
        /// </summary>
        Reject = 4
    }

    /// <summary>
    /// Order status
    /// </summary>
    public enum OrderStatus : byte
    {
        /// <summary>
        /// Active
        /// </summary>
        Active = 0,

        /// <summary>
        /// Executed (new trade)
        /// </summary>
        Trade = 1,

        /// <summary>
        /// Remove by user
        /// </summary>
        Remove = 2,

        /// <summary>
        /// End of time
        /// </summary>
        EndTime = 3,

        /// <summary>
        /// Reject by system
        /// </summary>
        Reject = 4
    }
}
