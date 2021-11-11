using Common;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pulxer.HistoryProvider
{
    internal struct TickerData
    {
        public string Ticker;
        public string ShortName;
        public string Name;
        public int LotSize;
        public byte Digits;
        public string Market;
        public string ID;
    }

    internal struct TimeframeData
    {
        public Timeframes Tf;
        public string TfKey;
        public string TfParam;
        public string Datf;
    }

    public class FinamHistoryProvider : IHistoryProvider
    {
        private List<TickerData> _tickers;
        private List<TimeframeData> _tfds;
        private string _baseUrl;
        private IRequester _requester;
        private readonly IConfig _config = null;
        private readonly ILogger _logger;

        public FinamHistoryProvider(IRequester req, IConfig config, ILogger<FinamHistoryProvider> logger)
        {
            _tfds = new List<TimeframeData>();
            _tickers = new List<TickerData>();
            _baseUrl = "";
            _requester = req;
            _config = config;
            _logger = logger;
        }

        public async Task Initialize()
        {
            _logger.LogInformation("Initialize.");

            _tfds.Clear(); _tickers.Clear();

            try 
            {
                XDocument xd = await LoadConfig();
                if (xd == null) return;

                // timeframes
                var tfNodes = xd.Root.Element("Timeframes").Elements("Timeframe");
                foreach (var tfNode in tfNodes)
                {
                    var tfAttr = tfNode.Attribute("Tf");
                    var tfKeyAttr = tfNode.Attribute("TfKey");
                    var tfParamAttr = tfNode.Attribute("TfParam");
                    var datfAttr = tfNode.Attribute("Datf");
                    if (tfAttr == null || tfKeyAttr == null || tfParamAttr == null || datfAttr == null)
                        continue;
                    int tf;
                    if (!Int32.TryParse(tfAttr.Value, out tf))
                        continue;

                    TimeframeData tfd = new TimeframeData()
                    {
                        Tf = (Timeframes)tf,
                        TfKey = tfKeyAttr.Value,
                        TfParam = tfParamAttr.Value,
                        Datf = datfAttr.Value
                    };
                    _tfds.Add(tfd);
                }

                // tickers
                var tickerNodes = xd.Root.Element("Tickers").Elements("Ticker");
                foreach (var tickerNode in tickerNodes)
                {
                    var tickerAttr = tickerNode.Attribute("Ticker");
                    var shortNameAttr = tickerNode.Attribute("ShortName");
                    var nameAttr = tickerNode.Attribute("Name");
                    var lotSizeAttr = tickerNode.Attribute("LotSize");
                    var digitsAttr = tickerNode.Attribute("Digits");
                    var marketAttr = tickerNode.Attribute("Market");
                    var idAttr = tickerNode.Attribute("ID");
                    if (tickerAttr == null || shortNameAttr == null || marketAttr == null || idAttr == null
                        || nameAttr == null || lotSizeAttr == null || digitsAttr == null)
                        continue;

                    int lotSize = 0;
                    if (!Int32.TryParse(lotSizeAttr.Value, out lotSize)) lotSize = 1;
                    byte digits = 0;
                    if (!Byte.TryParse(digitsAttr.Value, out digits)) digits = 0;
                    
                    TickerData td = new TickerData()
                    {
                        Ticker = tickerAttr.Value,
                        ShortName = shortNameAttr.Value,
                        Name = nameAttr.Value,
                        LotSize = lotSize,
                        Digits = digits,
                        Market = marketAttr.Value,
                        ID = idAttr.Value
                    };
                    _tickers.Add(td);
                }

                _baseUrl = xd.Root.Element("Url").Attribute("Base").Value;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Initialization error.");
                throw new Exception("Ошибка при инициализации провайдера исторических данных", ex);
            }
        }

        private async Task<XDocument> LoadConfig()
        {
            string conf = _config.GetHistoryProviderConfig();
            byte[] data = await File.ReadAllBytesAsync(conf);

            return XDocument.Load(new MemoryStream(data));
        }

        public IEnumerable<HistoryProviderTimeframe> GetTimeframes()
        {
            return (from tfd in _tfds select new HistoryProviderTimeframe(tfd.Tf)).ToList();
        }

        public IEnumerable<HistoryProviderInstrum> GetInstrums()
        {
            return (from t in _tickers 
                    select new HistoryProviderInstrum(t.Ticker, t.ShortName, t.Name, t.LotSize, t.Digits)).ToList();
        }

        public async Task<IEnumerable<Bar>> GetDataAsync(string ticker, Timeframes tf, DateTime date1, DateTime date2, int delay)
        {
            if (_requester == null) return null;
            if (_tickers.All(td => td.Ticker != ticker)) return null;
            if (_tfds.All(t => t.Tf != tf)) return null;

            byte[] data = ReadCache(ticker, tf, date1, date2);
            if (data == null)
            {
                string url = ReplParams(_baseUrl, ticker, tf, date1, date2);
                data = _requester.Request(url, delay);
                if (data != null) WriteCache(ticker, tf, date1, date2, data);
            }

            ParserSettings ps = new ParserSettings();
            ps.SkipLinesCount = 1;
            if (tf == Timeframes.Tick)
            {
                ps.OpenPriceIndex = ps.HighPriceIndex = ps.LowPriceIndex = ps.ClosePriceIndex = ps.VolumeIndex = -1;
                ps.FieldCount = 6;
            }
            else
            {
                ps.TickPriceIndex = ps.TickVolumeIndex = -1;
                ps.FieldCount = 9;
            }
            Parser parser = new Parser(ps);
            
            return await parser.ParseAsync(data);
        }

        /// <summary>
        /// Запись данных в локальный кэш
        /// </summary>
        /// <param name="ticker">Тикер</param>
        /// <param name="tf">Таймфрейм</param>
        /// <param name="date1">Дата 1</param>
        /// <param name="date2">Дата 2</param>
        /// <param name="data">Данные для записи</param>
        /// <returns>true - успешно, false - кэширование отключено</returns>
        private bool WriteCache(string ticker, Timeframes tf, DateTime date1, DateTime date2, byte[] data)
        {
            string cachePath = _config.GetHistoryProviderCache();
            if (string.IsNullOrEmpty(cachePath)) return false;

            if (!Directory.Exists(cachePath))
            {
                Directory.CreateDirectory(cachePath);
            }

            string path1 = Path.Combine(cachePath, ticker);
            if (!Directory.Exists(path1))
            {
                Directory.CreateDirectory(path1);
            }

            string path2 = Path.Combine(path1, tf.ToString());
            if (!Directory.Exists(path2))
            {
                Directory.CreateDirectory(path2);
            }

            string filename = string.Format("{0}_{1}.txt",
                date1.ToString("yyyy-MM-dd"),
                date2.ToString("yyyy-MM-dd"));

            if (data == null) data = new byte[0];

            File.WriteAllBytes(Path.Combine(path2, filename), data);

            return true;
        }

        /// <summary>
        /// Считать файл из кэша
        /// </summary>
        /// <param name="ticker">Тикер</param>
        /// <param name="tf">Таймфрейм</param>
        /// <param name="date1">Дата1</param>
        /// <param name="date2">Дата2</param>
        /// <returns>Чситанные данные или null</returns>
        private byte[] ReadCache(string ticker, Timeframes tf, DateTime date1, DateTime date2)
        {
            string cachePath = _config.GetHistoryProviderCache();
            if (string.IsNullOrEmpty(cachePath)) return null;

            string filename = string.Format("{0}_{1}.txt",
                date1.ToString("yyyy-MM-dd"),
                date2.ToString("yyyy-MM-dd"));
            string path = Path.Combine(cachePath, ticker, tf.ToString(), filename);

            byte[] data = null;
            try
            {
                data = File.ReadAllBytes(path);
            }
            catch
            {
            }

            return data;
        }

        /// <summary>
        /// Подстановка значений параметров в строку
        /// </summary>
        /// <param name="isStr">Исходная строка</param>
        /// <param name="ticker"></param>
        /// <param name="tf"></param>
        /// <param name="date1"></param>
        /// <param name="date2"></param>
        /// <returns>Результат</returns>
        private string ReplParams(string inStr,
            string ticker, Timeframes tf, DateTime date1, DateTime date2)
        {
            string outStr = inStr;
            TickerData tickerData = _tickers.FirstOrDefault(t => t.Ticker == ticker);
            TimeframeData tfData = _tfds.FirstOrDefault(t => t.Tf == tf);

            outStr = outStr.Replace("{TICKER}", ticker);
            outStr = outStr.Replace("{ID}", tickerData.ID);
            outStr = outStr.Replace("{Market}", tickerData.Market);
            outStr = outStr.Replace("{TfParam}", tfData.TfParam);
            outStr = outStr.Replace("{TFKey}", tfData.TfKey);
            outStr = outStr.Replace("{DATF}", tfData.Datf);

            outStr = outStr.Replace("{YYF}", date1.Date.Year.ToString().Substring(2, 2));
            outStr = outStr.Replace("{MMF}", date1.Date.Month.ToString("0#"));
            outStr = outStr.Replace("{DDF}", date1.Date.Day.ToString("0#"));
            outStr = outStr.Replace("{YYT}", date2.Date.Year.ToString().Substring(2, 2));
            outStr = outStr.Replace("{MMT}", date2.Date.Month.ToString("0#"));
            outStr = outStr.Replace("{DDT}", date2.Date.Day.ToString("0#"));

            outStr = outStr.Replace("{DF}", date1.Date.Day.ToString());
            outStr = outStr.Replace("{MF}", date1.Date.Month.ToString());
            outStr = outStr.Replace("{MF-1}", (date1.Date.Month - 1).ToString());
            outStr = outStr.Replace("{DF-1}", (date1.Date.Day - 1).ToString());
            outStr = outStr.Replace("{YYYYF}", date1.Date.Year.ToString());

            outStr = outStr.Replace("{DT}", date2.Date.Day.ToString());
            outStr = outStr.Replace("{MT}", date2.Date.Month.ToString());
            outStr = outStr.Replace("{MT-1}", (date2.Date.Month - 1).ToString());
            outStr = outStr.Replace("{DT-1}", (date2.Date.Day - 1).ToString());
            outStr = outStr.Replace("{YYYYT}", date2.Date.Year.ToString());

            return outStr;
        }
    }
}
