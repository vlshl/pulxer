using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    /// <summary>
    /// Tick history data
    /// </summary>
    public class DbTickHistory
    {
        public DbTickHistory()
        {
            TickHistoryID = 0;
            Date = DateTime.MinValue;
            InsID = 0;
            Data = new byte[0];
        }

        /// <summary>
        /// Identity
        /// </summary>
        public int TickHistoryID { get; set; }

        /// <summary>
        /// Date
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Instrum Id
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Binary data (all trades format)
        /// </summary>
        public byte[] Data { get; set; }
    }
}
