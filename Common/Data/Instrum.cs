using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    /// <summary>
    /// Financial instrument
    /// </summary>
    public class Instrum : IInstrum
    {
        public Instrum()
        {
            InsID = 0;
            Ticker = "";
            ShortName = "";
            Name = "";
            LotSize = 1;
            Decimals = 0;
            PriceStep = 0.01M;
        }

        /// <summary>
        /// Identity
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Ticker
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Short name
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        /// Full name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Lot size
        /// </summary>
        public int LotSize { get; set; }

        /// <summary>
        /// Price decimal places
        /// </summary>
        public int Decimals { get; set; }

        /// <summary>
        /// Price step
        /// </summary>
        public decimal PriceStep { get; set; }
    }

    /// <summary>
    /// Financial instrument list item
    /// </summary>
    public class InstrumListItem
    {
        public InstrumListItem()
        {
        }

        public InstrumListItem(int insID, string ticker, string shortName)
        {
            this.InsID = insID;
            this.Ticker = ticker;
            this.ShortName = shortName;
        }

        /// <summary>
        /// Identity
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Ticker
        /// </summary>
        public string Ticker { get; set; }

        /// <summary>
        /// Short name
        /// </summary>
        public string ShortName { get; set; }
    }
}
