using Common.Data;
using Platform;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PulxerTest
{
    public class PositionBLTest
    {
        private readonly IPositionBL _positionBL;

        public PositionBLTest()
        {
            var insBL = new InstrumBLMock();
            var posDA = new PositionDAMock();
            _positionBL = new PositionBL(posDA, insBL);
        }

        [Fact]
        public void GetSavePosTable_modifyTable_correctStore()
        {
            var table = _positionBL.GetPosTable(1); // ноль позиций, прогнали две сделки
            table.AddTrade(new Trade() { TradeID = 1, AccountID = 1, BuySell = BuySell.Buy, InsID = 1, LotCount = 10, OrderID = 1, Comm = 0, Price = 100, Time = new DateTime(2010, 1, 1, 10, 0, 0), TradeNo = 0 });
            table.AddTrade(new Trade() { TradeID = 2, AccountID = 1, BuySell = BuySell.Sell, InsID = 1, LotCount = 15, OrderID = 2, Comm = 0, Price = 110, Time = new DateTime(2010, 1, 2, 10, 0, 0), TradeNo = 0 });

            var positions = table.GetPositions(); // теперь один закрытый лонг и один открытый шорт
            var posTrades = table.GetPosTrades();
            Assert.Contains(positions, r => r.PosID <= 0); // есть PosID <= 0
            Assert.Contains(posTrades, r => r.PosID <= 0); // и тут тоже есть

            _positionBL.SavePosTable(table);

            var positions1 = table.GetPositions(); // не измелось, только PosID стали положительные после сохранения
            var posTrades1 = table.GetPosTrades();
            Assert.DoesNotContain(positions1, r => r.PosID <= 0); // теперь отрицательных PosID нет
            Assert.DoesNotContain(posTrades1, r => r.PosID <= 0); // и здесь нет

            table = _positionBL.GetPosTable(1); // поскольку одна позиция была закрыта, мы получаем только одну открытую позицию
            var positions2 = table.GetPositions();
            var posTrades2 = table.GetPosTrades(); // и сделка ей соответствует только одна

            Assert.DoesNotContain(positions2, r => r.PosID <= 0); // отрицательных PosID нет, поскольку было взято из базы
            Assert.DoesNotContain(posTrades2, r => r.PosID <= 0); // и здесь нет

            Assert.Equal(2, positions.Count());
            Assert.Equal(2, positions1.Count());
            Assert.Single(positions2); // только одна открытая позиция

            Assert.Equal(3, posTrades.Count());
            Assert.Equal(3, posTrades1.Count());
            Assert.Single(posTrades2); // и сделка только одна

            // закроем шорт
            table.AddTrade(new Trade() { TradeID = 3, AccountID = 1, BuySell = BuySell.Buy, InsID = 1, LotCount = 5, OrderID = 3, Comm = 0, Price = 120, Time = new DateTime(2010, 1, 3, 10, 0, 0), TradeNo = 0 });
            // сохраняем и извлекаем
            _positionBL.SavePosTable(table);
            table = _positionBL.GetPosTable(1); // нет открытых позиций

            var positions3 = table.GetPositions(); // поскольку все позиции закрыты, то ничего не извлеклось
            var posTrades3 = table.GetPosTrades();

            Assert.Empty(positions3); // все пусто
            Assert.Empty(posTrades3);
        }

        [Fact]
        public void RefreshPositions_()
        {
            // обработка сделок в правильном порядке в соответствии с их временем
            // в тесте специально сделки идут не по порядку
            PosDAMock posDAMock = new PosDAMock();
            var insBL = new InstrumBLMock();
            IPositionBL posBL = new PositionBL(posDAMock, insBL);
            posBL.RefreshPositions(1);

            var pos = posBL.GetAllPositions(1).ElementAt(0);

            Assert.Equal(PosTypes.Long, pos.PosType);
            Assert.Equal(100, pos.OpenPrice);
            Assert.Equal(110, pos.ClosePrice);
        }
    }

    internal class PosDAMock : PositionDAMock
    {
        public override IEnumerable<Trade> GetNewTrades(int accountID)
        {
            // специально перепутаны сделки
            // первой идет более поздняя, и TradeID у нее меньше
            List<Trade> trades = new List<Trade>();
            Trade t1 = new Trade() { AccountID = accountID, BuySell = BuySell.Buy, InsID = 1, LotCount = 10, OrderID = 0, Price = 100, Time = new DateTime(2010, 1, 1), Comm = 0, TradeID = 2, TradeNo = 111 };
            Trade t2 = new Trade() { AccountID = accountID, BuySell = BuySell.Sell, InsID = 1, LotCount = 10, OrderID = 0, Price = 110, Time = new DateTime(2010, 1, 2), Comm = 0, TradeID = 1, TradeNo = 222 };
            trades.Add(t2); trades.Add(t1);
            return trades;
        }

    }
}
