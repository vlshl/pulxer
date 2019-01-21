using Platform;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// The HistoryProvider gives history data
    /// </summary>
    public interface IHistoryProvider
    {
        Task Initialize();
        IEnumerable<HistoryProviderTimeframe> GetTimeframes();
        IEnumerable<HistoryProviderInstrum> GetInstrums();
        Task<IEnumerable<Bar>> GetDataAsync(string ticker, Timeframes tf, DateTime date1, DateTime date2);
    }

    /// <summary>
    /// Auxiliary object - history provider fin. instrument
    /// </summary>
    public class HistoryProviderInstrum
    {
        public HistoryProviderInstrum(string ticker, string shortName, string name, int lotSize, byte digits)
        {
            this.Ticker = ticker;
            this.ShortName = shortName;
            this.Name = name;
            this.LotSize = lotSize;
            this.Digits = digits;
        }

        /// <summary>
        /// Ticker
        /// </summary>
        public string Ticker { get; private set; }

        /// <summary>
        /// Short name
        /// </summary>
        public string ShortName { get; private set; }

        /// <summary>
        /// Full name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Lot size in units
        /// </summary>
        public int LotSize { get; private set; }

        /// <summary>
        /// Decimal places in price
        /// </summary>
        public byte Digits { get; private set; }
    }

    /// <summary>
    /// Auxiliary object - history provider timeframe
    /// </summary>
    public class HistoryProviderTimeframe
    {
        private Timeframes tf;
        private string[] names;

        public HistoryProviderTimeframe(Timeframes tf)
        {
            this.tf = tf;
            names = TfHelper.GetTimeframeNames(tf);
        }

        /// <summary>
        /// Timeframe
        /// </summary>
        public Timeframes Timeframe
        {
            get
            {
                return tf;
            }
        }

        /// <summary>
        /// Short name
        /// </summary>
        public string ShortName
        {
            get
            {
                return names[0];
            }
        }

        /// <summary>
        /// Full name
        /// </summary>
        public string Name
        {
            get
            {
                return names[1];
            }
        }
    }
}
