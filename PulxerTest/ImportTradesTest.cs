using Platform;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Xunit;

namespace PulxerTest
{
    public class ImportTradesTest
    {
        [Fact]
        public void ParseXml_incorrectXml_emptyTradeList()
        {
            InstrumBLMock instrumBL = new InstrumBLMock();
            TradesLoader importTrades = new TradesLoader(instrumBL, null);

            IEnumerable<Trade> trades;

            // null argument
            Assert.Throws<ArgumentNullException>(() => { importTrades.ParseXml(null); });

            // empty xml
            XmlDocument xd = new XmlDocument();
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // incorrect root element
            xd.LoadXml("<aaa />");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // empty trade list
            xd.LoadXml("<Trades />");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // no trade node
            xd.LoadXml("<Trades><aaa /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // empty trade node
            xd.LoadXml("<Trades><Trade /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);
        }

        [Fact]
        public void ParseXml_incorrectTradeNode_emptyTradeList()
        {
            InstrumBLMock instrumBL = new InstrumBLMock();
            TradesLoader importTrades = new TradesLoader(instrumBL, null);

            IEnumerable<Trade> trades;
            XmlDocument xd = new XmlDocument();

            // не все атрибуты есть
            xd.LoadXml("<Trades><Trade /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // Time есть, но он пустой
            xd.LoadXml("<Trades><Trade Time=\"\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // Time неверного формата
            xd.LoadXml("<Trades><Trade Time=\"qqqwwweee\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // Time неверного формата
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:90\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // Нет тикера
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // пустой тикер
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // неверный тикер
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"hello\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // нет цены
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // неверная цена
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // неверная цена
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"qqq.www\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // нет lots
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // пустой lots
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" Lots=\"\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // неверный lots
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" Lots=\"qqq\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // дробный lots
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" Lots=\"10.15\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // нет bs
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" Lots=\"10\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // пустой bs
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" Lots=\"10\" BS=\"\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // неверный bs
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" Lots=\"10\" BS=\"hello\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.Empty(trades);

            // все верно
            xd.LoadXml("<Trades><Trade Time=\"01.02.2010 10:20:30\" Ticker=\"ticker1\" Price=\"100.00\" Lots=\"10\" BS=\"buy\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.NotEmpty(trades);
            Trade trade = trades.ElementAt(0);
            Assert.Equal(new DateTime(2010, 2, 1, 10, 20, 30), trade.Time);
            Assert.Equal(100.00m, trade.Price);
            Assert.Equal(10, trade.LotCount);
            Assert.Equal(BuySell.Buy, trade.BuySell);
            Assert.Equal(1, trade.InsID);
        }

        /// <summary>
        /// Проверяем нижний и верхний регистр в атрибутах, а также запятую вместо точки как разделитель в числе
        /// </summary>
        [Fact]
        public void ParseXml_attrCharCaseAndCommaDecSeparator_correctAttributes()
        {
            InstrumBLMock instrumBL = new InstrumBLMock();
            TradesLoader importTrades = new TradesLoader(instrumBL, null);

            IEnumerable<Trade> trades;
            XmlDocument xd = new XmlDocument();

            xd.LoadXml("<Trades><Trade TiMe=\"01.02.2010 10:20:30\" TIckeR=\"ticker1\" pRice=\"100,00\" lotS=\"10\" bS=\"buy\" /></Trades>");
            trades = importTrades.ParseXml(xd);
            Assert.NotEmpty(trades);
            Trade trade = trades.ElementAt(0);
            Assert.Equal(new DateTime(2010, 2, 1, 10, 20, 30), trade.Time);
            Assert.Equal(100.00m, trade.Price);
            Assert.Equal(10, trade.LotCount);
            Assert.Equal(BuySell.Buy, trade.BuySell);
            Assert.Equal(1, trade.InsID);
        }
    }
}
