using Pulxer.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class VisualData
    {
        public int Key { get; set; }
        public string Side { get; set; }
        public string Type { get; set; }

        public static VisualData[] Generate(ChartData chartData)
        {
            var leftVisuals = chartData.GetVisuals(true);
            var rightVisuals = chartData.GetVisuals(false);

            List<VisualData> list = new List<VisualData>();
            foreach (var v in leftVisuals)
            {
                list.Add(new VisualData() { Key = v.Key, Side = "left", Type = v.Value.GetType().Name });
            }
            foreach (var v in rightVisuals)
            {
                list.Add(new VisualData() { Key = v.Key, Side = "right", Type = v.Value.GetType().Name });
            }

            return list.ToArray();
        }
    }
}
