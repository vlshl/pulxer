using Pulxer.Drawing;
using System.Collections.Generic;

namespace WebApp
{
    public class RemoteChartData
    {
        public bool IsDynamic { get; set; }
        public int Digits { get; set; }
        public RemoteVisual[] Visuals { get; set; }

        public static RemoteChartData Generate(ChartData chartData)
        {
            var leftVisuals = chartData.GetVisuals(true);
            var rightVisuals = chartData.GetVisuals(false);

            List<RemoteVisual> list = new List<RemoteVisual>();
            foreach (var v in leftVisuals)
            {
                list.Add(new RemoteVisual(v.Key, "left", v.Value.GetType().Name));
            }
            foreach (var v in rightVisuals)
            {
                list.Add(new RemoteVisual(v.Key, "right", v.Value.GetType().Name));
            }

            return new RemoteChartData() { IsDynamic = chartData.IsDynamic, Digits = chartData.Digits, Visuals = list.ToArray() };
        }
    }
}
