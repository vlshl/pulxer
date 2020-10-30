using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class RemoteTimeline
    {
        public DateTime StartTime { get; set; }
        public Timeframes Timeframe { get; set; }
        public int[] Increments { get; set; }

        public static RemoteTimeline Generate(Timeline timeline, int from, int? count = null)
        {
            if (from < 0) from = 0;
            if (count != null && count.Value < 0) count = 0;
            if (from >= timeline.Count) return null;
            int rest = timeline.Count - from;
            if (count == null || count.Value > rest) count = rest;
            List<int> incs = new List<int>();

            RemoteTimeline data = new RemoteTimeline();
            data.StartTime = timeline.Start(from).Value;
            data.Timeframe = timeline.Timeframe;
            
            long s = TimeSpan.TicksPerMinute;
            switch (timeline.Timeframe)
            {
                case Timeframes.Min: s = TimeSpan.TicksPerMinute; break;
                case Timeframes.Min5: s = TimeSpan.TicksPerMinute * 5; break;
                case Timeframes.Min10: s = TimeSpan.TicksPerMinute * 10; break;
                case Timeframes.Min15: s = TimeSpan.TicksPerMinute * 15; break;
                case Timeframes.Min20: s = TimeSpan.TicksPerMinute * 20; break;
                case Timeframes.Min30: s = TimeSpan.TicksPerMinute * 30; break;
                case Timeframes.Hour: s = TimeSpan.TicksPerHour; break;
                case Timeframes.Day: s = TimeSpan .TicksPerDay; break;
                case Timeframes.Week: s = TimeSpan.TicksPerDay * 7; break;
            }

            DateTime cur = data.StartTime;
            int lastInc = -1; int lastIncCount = 0;
            for (int i = from + 1; i < from + count; i++)
            {
                DateTime time = timeline.Start(i).Value;
                int inc = (int)((time.Ticks - cur.Ticks) / s);
                if (inc != lastInc && lastInc >= 0)
                {
                    incs.Add(lastInc);
                    incs.Add(lastIncCount);
                    lastIncCount = 1;
                }
                else
                {
                    lastIncCount++;
                }
                lastInc = inc;
                cur = time;
            }

            incs.Add(lastInc);
            incs.Add(lastIncCount);
            data.Increments = incs.ToArray();

            return data;
        }
    }
}
