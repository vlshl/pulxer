using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Pulxer.Drawing
{
    /// <summary>
    /// The continuous events row (positions for example)
    /// </summary>
    public class EventRow
    {
        private List<EventRowItem> _items;

        public EventRow()
        {
            _items = new List<EventRowItem>();
        }

        /// <summary>
        /// Add a new one-time event (start=end)
        /// </summary>
        /// <param name="time">Date/time or null</param>
        /// <param name="price">Price</param>
        /// <param name="eventType">Event type index</param>
        /// <returns>New item</returns>
        public EventRowItem Add(DateTime? time, decimal price, int eventType = 0)
        {
            return Add(time, time, price, eventType);
        }

        /// <summary>
        /// Add a new continuous event
        /// </summary>
        /// <param name="startTime">Start date/time or null</param>
        /// <param name="endTime">End date/time or null (included)</param>
        /// <param name="price">Price</param>
        /// <param name="eventType">Event type index</param>
        /// <returns>New item</returns>
        public EventRowItem Add(DateTime? startTime, DateTime? endTime, decimal price, int eventType = 0)
        {
            EventRowItem eri = new EventRowItem(startTime, endTime, price, eventType);
            lock (_items)
            {
                _items.Add(eri);
            }

            return eri;
        }

        /// <summary>
        /// Get events for period with partial or complete intersection
        /// </summary>
        /// <param name="startTime">Start date/time or null</param>
        /// <param name="endTime">End date/time or null</param>
        /// <returns>Events list</returns>
        public IEnumerable<EventRowItem> GetItems(DateTime? startTime, DateTime? endTime)
        {
            DateTime st = startTime != null ? startTime.Value : DateTime.MinValue;
            DateTime en = endTime != null ? endTime.Value : DateTime.MaxValue;

            lock (_items)
            {
                var list =
                    from r in _items
                    let s = r.StartTime != null ? r.StartTime : DateTime.MinValue
                    let e = r.EndTime != null ? r.EndTime : DateTime.MaxValue
                    where en >= s && st <= e
                    select r;
                return list.ToArray();
            }
        }
    }
}
