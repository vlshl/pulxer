using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public class Series
    {
        public int SeriesID { get; set; }
        public string Key { get; set; }
        public int AccountID { get; set; }
        public string Name { get; set; }
        public SeriesAxis Axis { get; set; }
        public string Data { get; set; }
    }

    public enum SeriesAxis
    {
        AxisX = 0, // значение не привязано к оси Y
        PriceAxis = 1, // значение привязано к ценовой оси Y (правой)
        LeftAxis = 2 // значение привязано к левой оси Y (не ценовой)
    }

    public class SeriesValue
    {
        public int SeriesValueID { get; set; }
        public int SeriesID { get; set; }
        public DateTime Time { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal Value { get; set; }
        public decimal? EndValue { get; set; }
        public string Data { get; set; }
    }
}
