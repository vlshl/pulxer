using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

namespace Pulxer
{
    public class TradesLoader
    {
        private readonly IInstrumBL _instrumBL = null;
        private readonly IAccountDA _accountDA = null;
        private StringBuilder sb;

        public TradesLoader(IInstrumBL instrumBL, IAccountDA accountDA)
        {
            _instrumBL = instrumBL;
            _accountDA = accountDA;
            sb = new StringBuilder();
        }

        public string Load(int accountID, string path)
        {
            sb.AppendLine("Load file ...");
            if (!File.Exists(path))
            {
                sb.AppendLine("File not found:" + path);
                return sb.ToString();
            }

            XmlDocument xd = new XmlDocument();
            try
            {
                string xml = File.ReadAllText(path);
                xd.LoadXml(xml);
            }
            catch (Exception ex)
            {
                sb.AppendLine(ex.ToString());
                return sb.ToString();
            }

            var trades = ParseXml(xd);
            if (trades.Any())
            {
                CreateTrades(accountID, trades);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Синтаксический разбор xml-документа со сделками
        /// </summary>
        /// <param name="xDoc">XML-документ</param>
        /// <returns>Список сделок</returns>
        public IEnumerable<Trade> ParseXml(XmlDocument xDoc)
        {
            if (xDoc == null)
                throw new ArgumentNullException();

            sb.AppendLine("Parsing ...");
            List<Trade> trades = new List<Trade>();

            if (xDoc.DocumentElement == null || xDoc.DocumentElement.Name.ToLower() != "trades")
            {
                sb.AppendLine("Incorrect root element");
                return trades;
            }

            CultureInfo ci = new CultureInfo("en-US");
            foreach (XmlNode xnTrade in xDoc.DocumentElement.ChildNodes)
            {
                if (xnTrade.Name.ToLower() != "trade")
                {
                    sb.AppendLine("Incorrect trade node: " + xnTrade.OuterXml);
                    continue;
                }

                var attribs = xnTrade.Attributes.Cast<XmlAttribute>().ToList();

                DateTime time;
                var xa_time = attribs.FirstOrDefault(r => r.Name.ToLower() == "time");
                if (xa_time == null || string.IsNullOrWhiteSpace(xa_time.Value))
                {
                    sb.AppendLine("Time attribute not found or empty: " + xnTrade.OuterXml);
                    continue;
                }
                if (!DateTime.TryParseExact(xa_time.Value, "dd.MM.yyyy HH:mm:ss", ci,  DateTimeStyles.None, out time))
                {
                    sb.AppendLine("Incorrect time (dd.mm.yyyy hh:mm:ss): " + xnTrade.OuterXml);
                    continue;
                }

                string ticker = "";
                var xa_ticker = attribs.FirstOrDefault(r => r.Name.ToLower() == "ticker");
                if (xa_ticker == null || string.IsNullOrWhiteSpace(xa_ticker.Value))
                {
                    sb.AppendLine("Ticker attribute not found or empty: " + xnTrade.OuterXml);
                    continue;
                }
                ticker = xa_ticker.Value;
                var instrum = _instrumBL.GetInstrum(ticker);
                if (instrum == null)
                {
                    sb.AppendLine("Ticker not found: " + xnTrade.OuterXml);
                    continue;
                }

                decimal price;
                var xa_price = attribs.FirstOrDefault(r => r.Name.ToLower() == "price");
                if (xa_price == null || string.IsNullOrWhiteSpace(xa_price.Value))
                {
                    sb.AppendLine("Price attribute not found or empty: " + xnTrade.OuterXml);
                    continue;
                }
                string p = xa_price.Value.Replace(",", "."); // в en-us используется точка, но мы понимаем и запятую
                if (!decimal.TryParse(p, NumberStyles.AllowDecimalPoint, ci, out price))
                {
                    sb.AppendLine("Incorrect price: " + xnTrade.OuterXml);
                    continue;
                }

                int lots;
                var xa_lots = attribs.FirstOrDefault(r => r.Name.ToLower() == "lots");
                if (xa_lots == null || string.IsNullOrWhiteSpace(xa_lots.Value))
                {
                    sb.AppendLine("Lots attribute not found or empty: " + xnTrade.OuterXml);
                    continue;
                }
                if (!int.TryParse(xa_lots.Value, NumberStyles.None, ci, out lots))
                {
                    sb.AppendLine("Incorrect lots: " + xnTrade.OuterXml);
                    continue;
                }

                BuySell bs;
                var xa_bs = attribs.FirstOrDefault(r => r.Name.ToLower() == "bs");
                if (xa_bs == null || string.IsNullOrWhiteSpace(xa_bs.Value))
                {
                    sb.AppendLine("BS attribute not found or empty: " + xnTrade.OuterXml);
                    continue;
                }
                if (xa_bs.Value.ToLower().StartsWith("b"))
                {
                    bs = BuySell.Buy;
                }
                else if (xa_bs.Value.ToLower().StartsWith("s"))
                {
                    bs = BuySell.Sell;
                }
                else
                {
                    sb.AppendLine("Incorrect BS: " + xnTrade.OuterXml);
                    continue;
                }

                Trade trade = new Trade()
                {
                    AccountID = 1,
                    BuySell = bs,
                    InsID = instrum.InsID,
                    LotCount = lots,
                    Comm = 0,
                    OrderID = 0,
                    Price = price,
                    Time = time,
                    TradeID = 0,
                    TradeNo = 0
                };
                trades.Add(trade);
            }
            sb.AppendLine("Parsing complete: " + trades.Count.ToString());

            return trades;
        }

        /// <summary>
        /// Создание сделок при импорте
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <param name="trades">Список сделок, которые нужно создать</param>
        /// <returns>Список созданных сделок (может отличаться от исходного списка)</returns>
        private IEnumerable<Trade> CreateTrades(int accountID, IEnumerable<Trade> trades)
        {
            sb.AppendLine("Create trades ...");
            var db_trades = _accountDA.GetTrades(accountID);
            List<Trade> createTrades = new List<Trade>();

            foreach (var trade in trades)
            {
                var found = db_trades.FirstOrDefault(r => r.BuySell == trade.BuySell && r.InsID == trade.InsID 
                    && r.LotCount == trade.LotCount && r.Price == trade.Price && r.Time == trade.Time);
                if (found != null)
                {
                    var instrum = _instrumBL.GetInstrumByID(trade.InsID);
                    if (instrum == null) continue;

                    sb.AppendLine(string.Format("Trade already exists: {0} {1} {2} {3} {4}", 
                        instrum.Ticker, 
                        trade.Time.ToString("dd.MM.yyyy HH:mm:ss"), 
                        trade.BuySell, 
                        trade.Price.ToString(), 
                        trade.LotCount.ToString()));
                    continue;
                }

                var order = _accountDA.CreateOrder(accountID, trade.Time, trade.InsID, trade.BuySell, trade.LotCount, trade.Price, OrderStatus.Trade, null, 0);
                var newTrade = _accountDA.CreateTrade(accountID, order.OrderID, trade.Time, trade.InsID, trade.BuySell, trade.LotCount, trade.Price, 0, 0);
                createTrades.Add(newTrade);
            }
            sb.AppendLine("Create trades complete: " + createTrades.Count.ToString());

            return createTrades;
        }
    }
}
