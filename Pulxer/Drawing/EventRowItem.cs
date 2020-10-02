using System;

namespace Pulxer.Drawing
{
    /// <summary>
    /// The continuous event
    /// </summary>
    public class EventRowItem
    {
        /// <summary>
        /// Start date and time of event
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// End date and time of event
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Event type index
        /// </summary>
        public int EventType { get; private set; }

        /// <summary>
        /// Price for axeY on price chart
        /// </summary>
        public decimal Price { get; private set; }

        public EventRowItem(DateTime? startTime, DateTime? endTime, decimal price) : this(startTime, endTime, price, 0)
        {
        }

        public EventRowItem(DateTime? time, decimal price) : this(time, time, price, 0)
        {
        }

        public EventRowItem(DateTime? time, decimal price, int eventType) : this(time, time, price, eventType)
        {
        }

        /// <summary>
        /// The continuous event
        /// </summary>
        /// <param name="startTime">Start date/time (included)</param>
        /// <param name="endTime">End date/time (included)</param>
        /// <param name="price">Price</param>
        /// <param name="eventType">Event type index</param>
        public EventRowItem(DateTime? startTime, DateTime? endTime, decimal price, int eventType)
        {
            StartTime = startTime;
            EndTime = endTime;
            Price = price;
            EventType = eventType;
        }
    }
}

