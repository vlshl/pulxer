using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Platform;

namespace Pulxer.HistoryProvider
{
    public class ParserSettings
    {
        public Encoding DataEncoding { get; set; }
        public string LineSeparator { get; set; }
        public int SkipLinesCount { get; set; }
        public string FieldSeparator { get; set; }
        public string NumberDecimalSeparator { get; set; }
        public string DateFormat { get; set; }
        public string TimeFormat { get; set; }
        public int FieldCount { get; set; }
        public bool IsTickData { get; set; }
        public Dictionary<string, Timeframes> Period2Timeframe;

        //public int TickerIndex { get; set; }
        //public int PeriodIndex { get; set; }
        public int DateIndex { get; set; }
        public int TimeIndex { get; set; }
        public int OpenPriceIndex { get; set; }
        public int HighPriceIndex { get; set; }
        public int LowPriceIndex { get; set; }
        public int ClosePriceIndex { get; set; }
        public int VolumeIndex { get; set; }
        public int TickPriceIndex { get; set; }
        public int TickVolumeIndex { get; set; }
        public NumberFormatInfo Nfi = null;

        public ParserSettings()
        {
            this.DataEncoding = Encoding.UTF8;
            this.LineSeparator = "\n";
            this.SkipLinesCount = 0;
            this.FieldSeparator = ",";
            this.NumberDecimalSeparator = ".";
            this.DateFormat = "yyyyMMdd";
            this.TimeFormat = "HHmmss";
            this.FieldCount = 0;
            this.IsTickData = false;

            this.Period2Timeframe = new Dictionary<string, Timeframes>();
            this.Period2Timeframe.Add("0", Timeframes.Tick);
            this.Period2Timeframe.Add("1", Timeframes.Min);
            this.Period2Timeframe.Add("5", Timeframes.Min5);
            this.Period2Timeframe.Add("10", Timeframes.Min10);
            this.Period2Timeframe.Add("15", Timeframes.Min15);
            this.Period2Timeframe.Add("20", Timeframes.Min20);
            this.Period2Timeframe.Add("30", Timeframes.Min30);
            this.Period2Timeframe.Add("60", Timeframes.Hour);
            this.Period2Timeframe.Add("D", Timeframes.Day);
            this.Period2Timeframe.Add("W", Timeframes.Week);

            //this.TickerIndex = 0;
            //this.PeriodIndex = 1;
            this.DateIndex = 2;
            this.TimeIndex = 3;
            this.OpenPriceIndex = 4;
            this.HighPriceIndex = 5;
            this.LowPriceIndex = 6;
            this.ClosePriceIndex = 7;
            this.VolumeIndex = 8;
            this.TickPriceIndex = 4;
            this.TickVolumeIndex = 5;
            this.Nfi = new NumberFormatInfo();
            this.Nfi.NumberDecimalSeparator = NumberDecimalSeparator;
        }
    }

    class Parser
    {
        private ParserSettings settings;

        public Parser()
        {
            this.settings = new ParserSettings();
        }

        public Parser(ParserSettings ps)
        {
            this.settings = ps;
        }

        public Task<IEnumerable<Bar>> ParseAsync(byte[] data)
        {
            return Task.Run<IEnumerable<Bar>>( () =>
            {
                string str = settings.DataEncoding.GetString(data, 0, data.Length);
                string[] lines = str.Split(new string[] { settings.LineSeparator }, StringSplitOptions.RemoveEmptyEntries)
                                    .Skip(settings.SkipLinesCount).ToArray();

                List<Bar> bars = new List<Bar>();
                foreach (string ln in lines)
                {
                    string line = ln.Trim();
                    if (line.Length == 0) continue;

                    string[] prms = line.Split(settings.FieldSeparator.ToCharArray());
                    if (prms.Length != settings.FieldCount) continue;

                    //string _ticker = settings.TickerIndex >= 0 ? prms[settings.TickerIndex] : "";
                    //string _per = settings.PeriodIndex >= 0 ? prms[settings.PeriodIndex] : "";
                    string _date = settings.DateIndex >= 0 ? prms[settings.DateIndex] : "";
                    string _time = settings.TimeIndex >= 0 ? prms[settings.TimeIndex] : "";

                    string _open = settings.OpenPriceIndex >= 0 ? prms[settings.OpenPriceIndex] : "";
                    string _high = settings.HighPriceIndex >= 0 ? prms[settings.HighPriceIndex] : "";
                    string _low = settings.LowPriceIndex >= 0 ? prms[settings.LowPriceIndex] : "";
                    string _close = settings.ClosePriceIndex >= 0 ? prms[settings.ClosePriceIndex] : "";
                    string _volume = settings.VolumeIndex >= 0 ? prms[settings.VolumeIndex] : "";
                    string _tickPrice = settings.TickPriceIndex >= 0 ? prms[settings.TickPriceIndex] : "";
                    string _tickVolume = settings.TickVolumeIndex >= 0 ? prms[settings.TickVolumeIndex] : "";

                    Bar bar = new Bar();

                    bar.Time = DateTime.ParseExact(_date + " " + _time, "yyyyMMdd HHmmss", settings.Nfi);
                    if (settings.IsTickData)
                    {
                        bar.Open = bar.High = bar.Low = bar.Close = decimal.Parse(_tickPrice, settings.Nfi);
                        if (_tickVolume != "")
                            bar.Volume = long.Parse(_tickVolume, settings.Nfi);
                    }
                    else
                    {
                        bar.Open = decimal.Parse(_open, settings.Nfi);
                        bar.High = decimal.Parse(_high, settings.Nfi);
                        bar.Low = decimal.Parse(_low, settings.Nfi);
                        bar.Close = decimal.Parse(_close, settings.Nfi);
                        if (_volume != "")
                            bar.Volume = long.Parse(_volume, settings.Nfi);
                    }

                    bars.Add(bar);
                }

                //if (bars.Count > 0)
                //    Logger.Write("Parsed: " + bars.Count.ToString() + " " + bars[0].Close.ToString());
                //else
                //    Logger.Write("Parsed: 0");

                return bars;
            });
        }
    }
}
