using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulxer.Drawing
{
    public class SeriesRow
    {
        private List<SeriesRowItem> _items;

        public SeriesRow()
        {
            _items = new List<SeriesRowItem>();
        }

        public void Add(DateTime time, decimal val, DateTime? endTime, decimal? endVal, string data)
        {
            var penSp = SeriesPropsUtil.Create(data) as PenSeriesProps;
            _items.Add(new SeriesRowItem() { Time = time, Value = val, EndTime = endTime, EndValue = endVal, Data = data, PenSp = penSp });
        }

        public IEnumerable<SeriesRowItem> GetItems(DateTime? startTime, DateTime? endTime)
        {
            DateTime st = startTime != null ? startTime.Value : DateTime.MinValue;
            DateTime en = endTime != null ? endTime.Value : DateTime.MaxValue;

            lock (_items)
            {
                var list =
                    from r in _items
                    let e = r.EndTime != null ? r.EndTime : r.Time
                    where en >= r.Time && st <= e
                    select r;

                return list.ToArray();
            }
        }
    }
}
