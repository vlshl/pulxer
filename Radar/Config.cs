using Indic;
using Newtonsoft.Json;
using Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Radar
{
    public class RadarConf
    {
        public string[] Tickers;
        public int DiffDecimals;
        public int MaDecimalsAdd;
        public MaConf Ma1;
        public MaConf Ma2;
        public MaConf Ma3;
        public string RedColor;
        public string GreenColor;
        public string Diff12Name;
        public string Diff13Name;
        public string Diff23Name;

        public RadarConf()
        {
            Tickers = new string[0];
            Ma1 = new MaConf();
            Ma2 = new MaConf();
            Ma3 = new MaConf();
            DiffDecimals = 3;
            MaDecimalsAdd = 1;
            RedColor = "red";
            GreenColor = "green";
            Diff12Name = string.Empty;
            Diff13Name = string.Empty;
            Diff23Name = string.Empty;
        }
    }

    public class MaConf
    {
        public string Name;
        public string DiffName;
        public Timeframes Timeframe;
        public AverageMethod Average;
        public int N;
        public int HistoryDays;

        public MaConf()
        {
            Name = "";
            DiffName = "";
            Timeframe = Timeframes.Min;
            Average = AverageMethod.Exponencial;
            N = 10;
            HistoryDays = 10;
        }
    }

    public class Config
    {
        private RadarConf _radarConf;
        private readonly IPluginPlatform _platform;
        private readonly string _pluginPath;

        public Config(IPluginPlatform platform, string path)
        {
            _platform = platform;
            _radarConf = new RadarConf();
            _pluginPath = path;
        }

        public RadarConf RadarConfig { get { return _radarConf; } }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this._radarConf, Formatting.Indented);
                File.WriteAllText(Path.Combine(_pluginPath, "config.json"), json);
            }
            catch(Exception ex)
            {
                _platform.AddLog("Radar.ConfigData.Save", ex.ToString());
            }
        }

        public bool Load()
        {
            try
            {
                string json = File.ReadAllText(Path.Combine(_pluginPath, "config.json"));
                _radarConf = JsonConvert.DeserializeObject<RadarConf>(json);
                return true;
            }
            catch (Exception ex)
            {
                _platform.AddLog("Radar.ConfigData.Load", ex.ToString());
                return false;
            }
        }
    }
}
