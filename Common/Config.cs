using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Common
{
    public class Config : IConfig
    {
        private IConfigurationSection _configSection;
        private Dictionary<string, int> _tf_delay;
        private Dictionary<string, int> _tf_days;
        private Dictionary<string, int> _tf_months;

        public Config(IConfiguration config)
        {
            _configSection = config.GetSection("Config");
            _tf_delay = new Dictionary<string, int>();
            _tf_days = new Dictionary<string, int>();
            _tf_months = new Dictionary<string, int>();
            LoadHistoryDownloadParts(config);
        }

        public string GetHistoryProviderConfig()
        {
            return _configSection["HistoryProviderConfig"];
        }

        public string GetHistoryProviderCache()
        {
            return _configSection["HistoryProviderCache"];
        }

        public string GetBotsPath()
        {
            return _configSection["BotsPath"];
        }

        public string GetPluginsPath()
        {
            return _configSection["PluginsPath"];
        }

        public string GetTickHistoryPath()
        {
            return _configSection["TickHistoryPath"];
        }

        public int GetHistoryDownloaderDays(string tf)
        {
            tf = tf.ToLower();
            if (!_tf_days.ContainsKey(tf)) return 1;

            return _tf_days[tf];
        }

        public int GetHistoryDownloaderMonths(string tf)
        {
            tf = tf.ToLower();
            if (!_tf_months.ContainsKey(tf)) return 0;

            return _tf_months[tf];
        }

        public int GetHistoryDownloaderDelay(string tf)
        {
            tf = tf.ToLower();
            if (!_tf_delay.ContainsKey(tf)) return 0;

            return _tf_delay[tf];
        }

        private void LoadHistoryDownloadParts(IConfiguration config)
        {
            _tf_delay.Clear(); _tf_days.Clear(); _tf_months.Clear();

            try
            {
                var partsSection = config.GetSection("HistoryDownloader").GetSection("Parts");

                int days; int months; int delay;
                var parts = partsSection.GetChildren();
                foreach (var part in parts)
                {
                    var tfs = Regex.Split(part["Timeframes"], @"\s*,\s*");
                    if (!int.TryParse(part["Days"], out days)) days = 1;
                    if (!int.TryParse(part["Months"], out months)) months = 0;
                    if (!int.TryParse(part["Delay"], out delay)) delay = 0;

                    foreach (var tf in tfs)
                    {
                        _tf_delay.Add(tf.ToLower(), delay);
                        _tf_days.Add(tf.ToLower(), days);
                        _tf_months.Add(tf.ToLower(), months);
                    }
                }
            }
            catch(Exception ex) { }
        }
    }
}
