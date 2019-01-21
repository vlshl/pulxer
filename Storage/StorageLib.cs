using System;

namespace Storage
{
    /// <summary>
    /// Helper functions library
    /// </summary>
    public static class StorageLib
    {
        private static long _t0 = new DateTime(2000, 1, 1).Ticks;

        /// <summary>
        /// DateTime to int (seconds from 1.1.2000)
        /// </summary>
        /// <param name="date">Date and time or null</param>
        /// <returns>Int value or null</returns>
        public static int? ToDbTime(DateTime? date)
        {
            if (date == null) 
                return null; 
            else 
                return ToDbTime(date.Value);
        }

        /// <summary>
        /// DateTime to int (seconds from 1.1.2000)
        /// </summary>
        /// <param name="date">Date and time not null</param>
        /// <returns>Int value</returns>
        public static int ToDbTime(DateTime date)
        {
            long secs = (date.Ticks - _t0) / TimeSpan.TicksPerSecond;
            if (secs < int.MinValue) return int.MinValue;
            if (secs > int.MaxValue) return int.MaxValue;

            return (int)secs;
        }

        /// <summary>
        /// Seconds from 1.1.2000 to date/time
        /// </summary>
        /// <param name="dbTime">Seconds from 1.1.2000 or null</param>
        /// <returns>DateTime or null</returns>
        public static DateTime? ToDateTime(int? dbTime)
        {
            if (dbTime == null) 
                return null; 
            else 
                return ToDateTime(dbTime.Value);
        }

        /// <summary>
        /// Seconds from 1.1.2000 to date/time
        /// </summary>
        /// <param name="dbTime">Seconds from 1.1.2000 not null</param>
        /// <returns>DateTime</returns>
        public static DateTime ToDateTime(int dbTime)
        {
            return new DateTime(dbTime * TimeSpan.TicksPerSecond + _t0);
        }
    }
}
