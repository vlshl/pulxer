using Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    /// <summary>
    /// This object incapsulates continuous historical data storage periods
    /// and free days (weekends)
    /// </summary>
    public class InsStoreCalendar
    {
        private List<InsStorePeriod> _periods = null;
        private List<DateTime> _freeDays = null;

        public InsStoreCalendar()
        {
            _periods = new List<InsStorePeriod>();
            _freeDays = new List<DateTime>();
        }

        /// <summary>
        /// Append new storage period and optimize (merge) periods
        /// </summary>
        /// <param name="period">New continuous storage period</param>
        public void AppendPeriod(InsStorePeriod period)
        {
            if (period == null) return;

            _periods.Add(period);
            OptimizePeriods();
        }

        /// <summary>
        /// Append new weekends
        /// </summary>
        /// <param name="freeDays">Weekends list</param>
        /// <param name="date1">Start period</param>
        /// <param name="date2">End period</param>
        public void UpdateFreeDays(IEnumerable<DateTime> freeDays, DateTime date1, DateTime date2)
        {
            _freeDays.RemoveAll(d => d >= date1 && d <= date2);
            AddFreeDays(freeDays);
        }

        /// <summary>
        /// Append new weekends
        /// </summary>
        /// <param name="freeDays">Weekends list</param>
        public void AddFreeDays(IEnumerable<DateTime> freeDays)
        {
            if (freeDays == null) return;

            foreach (var day in freeDays)
            {
                if (_freeDays.Contains(day.Date)) continue;
                _freeDays.Add(day.Date);
            }
        }

        /// <summary>
        /// Get storage periods
        /// </summary>
        public IEnumerable<InsStorePeriod> Periods
        {
            get
            {
                return _periods.ToList();
            }
        }

        /// <summary>
        /// Get weekends
        /// </summary>
        public IEnumerable<DateTime> FreeDays
        {
            get
            {
                return _freeDays;
            }
        }

        private void OptimizePeriods()
        {
            while (true)
            {
            loop:
                if (_periods.Count < 2) break;

                for (int i = 0; i < _periods.Count; i++)
                    for (int j = i + 1; j < _periods.Count; j++)
                    {
                        // if period is dirty, the end date shift one day ago
                        // if period became empty after that, skip processing
                        var p1 = _periods[i]; var p2 = _periods[j];
                        DateTime p1_end = p1.IsLastDirty ? p1.EndDate.AddDays(-1) : p1.EndDate; // skip dirty day
                        DateTime p2_end = p2.IsLastDirty ? p2.EndDate.AddDays(-1) : p2.EndDate;
                        bool isOnedayDirty1 = p1.StartDate > p1_end;
                        bool isOnedayDirty2 = p2.StartDate > p2_end;
                        if ((isOnedayDirty1 || isOnedayDirty2) && (p1.EndDate == p2.EndDate)) // dirty one-day periods with equal end date
                        {
                            if (isOnedayDirty1) // if the first period is one-day - delete it
                                _periods.Remove(p1);
                            else if (isOnedayDirty2) // the first period is not one-day, and the second perion is one-day - delete the second period
                                _periods.Remove(p2);
                            goto loop;
                        }
                        else
                        {
                            if ((p2.StartDate <= p1_end && p1.StartDate <= p2_end) // cross
                                || (p1_end.AddDays(1) == p2.StartDate || p2_end.AddDays(1) == p1.StartDate)) // not cross but may be combined
                            {
                                DateTime s = p1.StartDate < p2.StartDate ? p1.StartDate : p2.StartDate;
                                DateTime e = p1.EndDate > p2.EndDate ? p1.EndDate : p2.EndDate;
                                bool isDirty = p1.EndDate > p2.EndDate ? p1.IsLastDirty : p2.IsLastDirty;
                                _periods.Remove(p1); _periods.Remove(p2);
                                _periods.Add(new InsStorePeriod(s, e, isDirty));
                                goto loop;
                            }
                        }
                    }
                break;
            }
        }
    }
}
