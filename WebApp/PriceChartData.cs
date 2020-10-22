using Pulxer.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class PriceChartData
    {
        public ChartBrush Brush { get; set; }
        public long StartPrice { get; set; }
        public int Decimals { get; set; }
        public long[] Diffs { get; set; }

        public static PriceChartData Generate(int key, int decimals, ChartData chartData)
        {
            var pc = chartData.GetVisual(key) as PriceChart;
            if (pc == null) return null;

            var data = new PriceChartData();
            var br = pc.GetBars();
            if (br == null) return null;

            data.Brush = pc.GetBrush();
            data.StartPrice = 0;
            data.Diffs = new long[0];
            data.Decimals = decimals;

            int k = 1;
            if (decimals == 1) k = 10;
            else if (decimals == 2) k = 100;
            else if (decimals == 3) k = 1000;
            else if (decimals == 4) k = 10000;
            else if (decimals == 5) k = 100000;
            else if (decimals == 6) k = 1000000;
            else if (decimals == 7) k = 10000000;
            else if (decimals == 8) k = 100000000;
            else if (decimals == 9) k = 1000000000;
            else if (decimals == 10) k = 1000000000;

            var bars = br.Bars;
            if (!bars.Any()) return data;

            data.StartPrice = (long)(bars.First().Open * k);

            List<long> dfs = new List<long>();
            long curPrice = data.StartPrice;

            long p;
            foreach (var bar in bars)
            {
                p = (long)(bar.Open * k); dfs.Add(p - curPrice); curPrice = p;
                p = (long)(bar.High * k); dfs.Add(p - curPrice); curPrice = p;
                p = (long)(bar.Low * k); dfs.Add(p - curPrice); curPrice = p;
                p = (long)(bar.Close * k); dfs.Add(p - curPrice); curPrice = p;
                dfs.Add(bar.Volume);
            }
            data.Diffs = dfs.ToArray();

            return data;
        }
    }
}
