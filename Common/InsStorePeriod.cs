using System;

namespace Common
{
    /// <summary>
    /// Continuous period of historical data storage
    /// </summary>
    public class InsStorePeriod
    {
        /// <summary>
        /// Start date of period (the first day)
        /// </summary>
        public DateTime StartDate { get; private set; }

        /// <summary>
        /// End date of period (the last day)
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// The last price bar needs to be correct
        /// </summary>
        public bool IsLastDirty { get; private set; }

        public InsStorePeriod(DateTime start, DateTime end, bool isLastDirty = false)
        {
            if (end < start)
                throw new ArgumentException();
            StartDate = start.Date;
            EndDate = end.Date;
            IsLastDirty = isLastDirty;
        }
    }
}
