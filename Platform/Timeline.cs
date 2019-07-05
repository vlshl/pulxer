using System;
using System.Collections.Generic;

namespace Platform
{
    /// <summary>
    /// The timeline.
    /// Each time interval contains start time and start time of the next interval.
    /// </summary>
    public class Timeline
    {
        private List<BarDate> _dates = new List<BarDate>();
        private Timeframes _tf;

        public Timeline(Timeframes tf)
        {
            _tf = tf;
        }

        /// <summary>
        /// The Timeframes enum.
        /// </summary>
        public Timeframes Timeframe
        {
            get
            {
                return _tf;
            }
        }

        /// <summary>
        /// Get start time of the time interval.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DateTime? Start(int index)
        {
            if (index < 0 || index >= _dates.Count) return null;

            return _dates[index].Start;
        }

        /// <summary>
        /// Get start time of the next time interval.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public DateTime? NextStart(int index)
        {
            if (index < 0 || index >= _dates.Count) return null;

            return _dates[index].NextStart;
        }

        /// <summary>
        /// Get the BarDate struct by index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public BarDate? GetBarDate(int index)
        {
            if (index < 0 || index >= _dates.Count) return null;

            return _dates[index];
        }

        /// <summary>
        /// Add new time interval.
        /// </summary>
        /// <param name="date">Start time</param>
        /// <param name="tf">Timeframe</param>
        public void Add(DateTime date, Timeframes tf)
        {
            DateTime[] dts = TfHelper.GetDates(date, tf);
            _dates.Add(new BarDate(dts[0], dts[1]));
        }

        /// <summary>
        /// Add new time interval.
        /// </summary>
        /// <param name="start">Start time</param>
        /// <param name="end">Start time of the next interval</param>
        public void Add(DateTime start, DateTime end)
        {
            _dates.Add(new BarDate(start, end));
        }

        /// <summary>
        /// Count of intervals.
        /// </summary>
        public int Count
        {
            get
            {
                return _dates.Count;
            }
        }

        /// <summary>
        /// Is correct index?
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsExists(int index)
        {
            return index >= 0 && index < _dates.Count;
        }

        /// <summary>
        /// Find index by time
        /// </summary>
        /// <param name="time">Time</param>
        /// <returns>Found index or -1</returns>
        public int FindIndex(DateTime time)
        {
            return _dates.FindIndex(d => d.Start <= time && time < d.NextStart);
        }

        /// <summary>
        /// Clear timeline.
        /// </summary>
        public void Clear()
        {
            _dates.Clear();
        }
    }

    /// <summary>
    /// BarDate struct contains start time of interval and start time of the next interval.
    /// </summary>
    public struct BarDate
    {
        public DateTime Start { get; private set; }
        public DateTime NextStart { get; private set; }

        public BarDate(DateTime start, DateTime nextStart)
        {
            Start = start; NextStart = nextStart;
        }
    }
}
