using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Pulxer;
using Common.Interfaces;
using Platform;
using Common.Data;

namespace PulxerTest
{
    public class PosTableTest
    {
        private IInstrumBL instrumBL = new InstrumBLMock();

        [Fact]
        public void AddTrade_buyTrade_singleOpenLongPosition()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 1, Time = new DateTime(2010, 1, 1, 10, 30, 30), Price = 100m, LotCount = 5, BuySell = BuySell.Buy };
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.Single(positions);
            var pos = positions.ElementAt(0);

            Assert.Equal(t.InsID, pos.InsID);
            Assert.True(pos.PosID < 0);
            Assert.Equal(t.Time, pos.OpenTime);
            Assert.Equal(t.Price, pos.OpenPrice);
            Assert.Equal(t.LotCount * ins.LotSize, pos.Count);
            Assert.Equal(PosTypes.Long, pos.PosType);
            Assert.Null(pos.ClosePrice);
            Assert.Null(pos.CloseTime);
        }

        [Fact]
        public void AddTrade_sellTrade_singleOpenShortPosition()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 1, Time = new DateTime(2010, 1, 1, 10, 30, 30), Price = 100m, LotCount = 5, BuySell = BuySell.Sell };
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.Single(positions);
            var pos = positions.ElementAt(0);

            Assert.Equal(t.InsID, pos.InsID);
            Assert.True(pos.PosID < 0);
            Assert.Equal(t.Time, pos.OpenTime);
            Assert.Equal(t.Price, pos.OpenPrice);
            Assert.Equal(t.LotCount * ins.LotSize, pos.Count);
            Assert.Equal(PosTypes.Short, pos.PosType);
            Assert.Null(pos.ClosePrice);
            Assert.Null(pos.CloseTime);
        }

        [Fact]
        public void AddTrade_buyAndSell_singleCloseLongPosition()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            Trade t1 = new Trade() { InsID = ins.InsID, OrderID = 1, Time = new DateTime(2010, 1, 1, 10, 30, 30), Price = 100m, LotCount = 5, BuySell = BuySell.Buy };
            table.AddTrade(t1);

            Trade t2 = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 5, BuySell = BuySell.Sell };
            table.AddTrade(t2);

            var positions = table.GetPositions();
            Assert.Single(positions);
            var pos = positions.ElementAt(0);

            Assert.Equal(t1.InsID, pos.InsID);
            Assert.True(pos.PosID < 0);
            Assert.Equal(t1.Time, pos.OpenTime);
            Assert.Equal(t1.Price, pos.OpenPrice);
            Assert.Equal(t1.LotCount * ins.LotSize, pos.Count);
            Assert.Equal(PosTypes.Long, pos.PosType);
            Assert.Equal(t2.Time, pos.CloseTime);
            Assert.Equal(t2.Price, pos.ClosePrice);
        }

        [Fact]
        public void AddTrade_sellAndBuy_singleCloseShortPosition()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            Trade t1 = new Trade() { InsID = ins.InsID, OrderID = 1, Time = new DateTime(2010, 1, 1, 10, 30, 30), Price = 100m, LotCount = 5, BuySell = BuySell.Sell };
            table.AddTrade(t1);

            Trade t2 = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 5, BuySell = BuySell.Buy };
            table.AddTrade(t2);

            var positions = table.GetPositions();
            Assert.Single(positions);
            var pos = positions.ElementAt(0);

            Assert.Equal(t1.InsID, pos.InsID);
            Assert.True(pos.PosID < 0);
            Assert.Equal(t1.Time, pos.OpenTime);
            Assert.Equal(t1.Price, pos.OpenPrice);
            Assert.Equal(t1.LotCount * ins.LotSize, pos.Count);
            Assert.Equal(PosTypes.Short, pos.PosType);
            Assert.Equal(t2.Time, pos.CloseTime);
            Assert.Equal(t2.Price, pos.ClosePrice);
        }

        [Fact]
        public void AddTrade_openLongPosAndSell_closedLongPos()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime dt = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p = table.NewPosition(ins.InsID, 10 * ins.LotSize, dt, 100, PosTypes.Long);
            table.Initialize(1, new List<Position>() { p }, null);

            Assert.False(p.IsChanged); // не изменена

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 10, BuySell = BuySell.Sell };
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.Single(positions);
            var pos = positions.ElementAt(0);

            Assert.Equal(dt, pos.OpenTime);
            Assert.Equal(100, pos.OpenPrice);
            Assert.Equal(10 * ins.LotSize, pos.Count);
            Assert.Equal(PosTypes.Long, pos.PosType);
            Assert.Equal(t.Time, pos.CloseTime);
            Assert.Equal(t.Price, pos.ClosePrice);
            Assert.True(pos.IsChanged); // изменена
        }

        [Fact]
        public void AddTrade_openShortPosAndBuy_closedShortPos()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime dt = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p = table.NewPosition(ins.InsID, 10 * ins.LotSize, dt, 100, PosTypes.Short);
            table.Initialize(1, new List<Position>() { p }, null);

            Assert.False(p.IsChanged); // не изменена

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 10, BuySell = BuySell.Buy };
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.Single(positions);
            var pos = positions.ElementAt(0);

            Assert.Equal(dt, pos.OpenTime);
            Assert.Equal(100, pos.OpenPrice);
            Assert.Equal(10 * ins.LotSize, pos.Count);
            Assert.Equal(PosTypes.Short, pos.PosType);
            Assert.Equal(t.Time, pos.CloseTime);
            Assert.Equal(t.Price, pos.ClosePrice);
            Assert.True(pos.IsChanged); // изменена
        }

        /// <summary>
        /// Одна позиция была частично закрыта
        /// Получилось две позиции
        /// </summary>
        [Fact]
        public void AddTrade_openLongPosAndSell_splitLongPos()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime posDt = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p = table.NewPosition(ins.InsID, 10 * ins.LotSize, posDt, 100, PosTypes.Long);
            table.Initialize(1, new List<Position>() { p }, null); // есть позиция long на 10

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 3, BuySell = BuySell.Sell }; // продали всего 3
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.True(positions.Count() == 2); // стало 2 позиции
            var pos1 = positions.ElementAt(0);
            var pos2 = positions.ElementAt(1);

            Assert.Equal(posDt, pos1.OpenTime);
            Assert.Equal(100, pos1.OpenPrice);
            Assert.Equal(3 * ins.LotSize, pos1.Count);
            Assert.Equal(PosTypes.Long, pos1.PosType);
            Assert.Equal(t.Time, pos1.CloseTime);
            Assert.Equal(t.Price, pos1.ClosePrice);
            Assert.True(pos1.IsChanged);

            Assert.Equal(posDt, pos2.OpenTime);
            Assert.Equal(100, pos2.OpenPrice);
            Assert.Equal(7 * ins.LotSize, pos2.Count);
            Assert.Equal(PosTypes.Long, pos2.PosType);
            Assert.Null(pos2.CloseTime);
            Assert.Null(pos2.ClosePrice);
            Assert.True(pos2.PosID < 0);
        }

        /// <summary>
        /// Одна позиция была частично закрыта
        /// Получилось две позиции
        /// </summary>
        [Fact]
        public void AddTrade_openShortPosAndBuy_splitShortPos()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime posDt = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p = table.NewPosition(ins.InsID, 10 * ins.LotSize, posDt, 100, PosTypes.Short);
            table.Initialize(1, new List<Position>() { p }, null); // есть позиция short на 10

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 3, BuySell = BuySell.Buy }; // купили всего 3
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.True(positions.Count() == 2); // стало 2 позиции
            var pos1 = positions.ElementAt(0);
            var pos2 = positions.ElementAt(1);

            Assert.Equal(posDt, pos1.OpenTime);
            Assert.Equal(100, pos1.OpenPrice);
            Assert.Equal(3 * ins.LotSize, pos1.Count);
            Assert.Equal(PosTypes.Short, pos1.PosType);
            Assert.Equal(t.Time, pos1.CloseTime);
            Assert.Equal(t.Price, pos1.ClosePrice);
            Assert.True(pos1.IsChanged);

            Assert.Equal(posDt, pos2.OpenTime);
            Assert.Equal(100, pos2.OpenPrice);
            Assert.Equal(7 * ins.LotSize, pos2.Count);
            Assert.Equal(PosTypes.Short, pos2.PosType);
            Assert.Null(pos2.CloseTime);
            Assert.Null(pos2.ClosePrice);
            Assert.True(pos2.PosID < 0);
        }

        /// <summary>
        /// Одна позиция была перекрыта
        /// Получилось две позиции
        /// </summary>
        [Fact]
        public void AddTrade_openLongPosAndSell_closedLongAndNewShort()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime posDt = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p = table.NewPosition(ins.InsID, 10 * ins.LotSize, posDt, 100, PosTypes.Long);
            table.Initialize(1, new List<Position>() { p }, null); // есть позиция long на 10

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 12, BuySell = BuySell.Sell }; // продали 12
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.True(positions.Count() == 2); // стало 2 позиции
            var pos1 = positions.ElementAt(0);
            var pos2 = positions.ElementAt(1);

            Assert.Equal(posDt, pos1.OpenTime); // прежняя позиция полностью закрыта
            Assert.Equal(100, pos1.OpenPrice);
            Assert.Equal(10 * ins.LotSize, pos1.Count);
            Assert.Equal(PosTypes.Long, pos1.PosType);
            Assert.Equal(t.Time, pos1.CloseTime);
            Assert.Equal(t.Price, pos1.ClosePrice);
            Assert.True(pos1.IsChanged);

            Assert.Equal(t.Time, pos2.OpenTime); // и противоположная новая открыта 
            Assert.Equal(110m, pos2.OpenPrice); // время и цена сделки, а не прежней позиции
            Assert.Equal(2 * ins.LotSize, pos2.Count); // остаток
            Assert.Equal(PosTypes.Short, pos2.PosType); // стала short
            Assert.Null(pos2.CloseTime); // открыта
            Assert.Null(pos2.ClosePrice);
            Assert.True(pos2.PosID < 0); // новая
        }

        /// <summary>
        /// Одна позиция была перекрыта
        /// Получилось две позиции
        /// </summary>
        [Fact]
        public void AddTrade_openShortPosAndBuy_closedShortAndNewLong()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime posDt = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p = table.NewPosition(ins.InsID, 10 * ins.LotSize, posDt, 100, PosTypes.Short);
            table.Initialize(1, new List<Position>() { p }, null); // есть позиция short на 10

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2010, 1, 1, 10, 30, 35), Price = 110m, LotCount = 12, BuySell = BuySell.Buy }; // купили 12
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.True(positions.Count() == 2); // стало 2 позиции
            var pos1 = positions.ElementAt(0);
            var pos2 = positions.ElementAt(1);

            Assert.Equal(posDt, pos1.OpenTime); // прежняя позиция полностью закрыта
            Assert.Equal(100, pos1.OpenPrice);
            Assert.Equal(10 * ins.LotSize, pos1.Count);
            Assert.Equal(PosTypes.Short, pos1.PosType);
            Assert.Equal(t.Time, pos1.CloseTime);
            Assert.Equal(t.Price, pos1.ClosePrice);
            Assert.True(pos1.IsChanged);

            Assert.Equal(t.Time, pos2.OpenTime); // и противоположная новая открыта 
            Assert.Equal(110m, pos2.OpenPrice); // время и цена сделки, а не прежней позиции
            Assert.Equal(2 * ins.LotSize, pos2.Count); // остаток
            Assert.Equal(PosTypes.Long, pos2.PosType); // стала long
            Assert.Null(pos2.CloseTime); // открыта
            Assert.Null(pos2.ClosePrice);
            Assert.True(pos2.PosID < 0); // новая
        }

        /// <summary>
        /// Было два лонга и сделали большую продажу
        /// так что оба лонга закрытись и еще шорт открылся
        /// </summary>
        [Fact]
        public void AddTrade_openTwoLongsAndSell_closedTwoLongsAndNewShort()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime dt1 = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p1 = table.NewPosition(ins.InsID, 10 * ins.LotSize, dt1, 100, PosTypes.Long);
            DateTime dt2 = new DateTime(2011, 1, 1, 10, 30, 0);
            Position p2 = table.NewPosition(ins.InsID, 20 * ins.LotSize, dt2, 120, PosTypes.Long);
            table.Initialize(1, new List<Position>() { p2, p1 }, null); // две позиции, добавляем специально не по порядку дат

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2012, 1, 1, 10, 30, 35), Price = 110m, LotCount = 35, BuySell = BuySell.Sell }; // sell 35
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.True(positions.Count() == 3); // стало 3 позиции
            var pos1 = positions.ElementAt(0);
            var pos2 = positions.ElementAt(1);
            var pos3 = positions.ElementAt(2);

            Assert.Equal(dt1, pos1.OpenTime); // первая закрыта (по порядку дат)
            Assert.Equal(100, pos1.OpenPrice);
            Assert.Equal(10 * ins.LotSize, pos1.Count);
            Assert.Equal(PosTypes.Long, pos1.PosType);
            Assert.Equal(t.Time, pos1.CloseTime);
            Assert.Equal(t.Price, pos1.ClosePrice);
            Assert.True(pos1.IsChanged);

            Assert.Equal(dt2, pos2.OpenTime); // вторая закрыта (по порядку дат)
            Assert.Equal(120, pos2.OpenPrice);
            Assert.Equal(20 * ins.LotSize, pos2.Count);
            Assert.Equal(PosTypes.Long, pos2.PosType);
            Assert.Equal(t.Time, pos2.CloseTime);
            Assert.Equal(t.Price, pos2.ClosePrice);
            Assert.True(pos2.IsChanged);

            Assert.Equal(t.Time, pos3.OpenTime); // третья открыта
            Assert.Equal(t.Price, pos3.OpenPrice);
            Assert.Equal(5 * ins.LotSize, pos3.Count);
            Assert.Equal(PosTypes.Short, pos3.PosType);
            Assert.Null(pos3.CloseTime);
            Assert.Null(pos3.ClosePrice);
            Assert.True(pos3.PosID < 0);
        }

        /// <summary>
        /// Было два лонга и сделали продажу, которая один лонг полностью перекрыла, а второй частично
        /// один лонг закрылся, а второй разделился на два
        /// </summary>
        [Fact]
        public void AddTrade_openTwoLongsAndSell_closedOneLongAndSplit()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            DateTime dt1 = new DateTime(2010, 1, 1, 10, 30, 0);
            Position p1 = table.NewPosition(ins.InsID, 10 * ins.LotSize, dt1, 100, PosTypes.Long); // 10

            DateTime dt2 = new DateTime(2011, 1, 1, 10, 30, 0);
            Position p2 = table.NewPosition(ins.InsID, 20 * ins.LotSize, dt2, 120, PosTypes.Long); // 20
            table.Initialize(1, new List<Position>() { p2, p1 }, null); // две позиции, добавляем специально не по порядку дат

            Trade t = new Trade() { InsID = ins.InsID, OrderID = 2, Time = new DateTime(2012, 1, 1, 10, 30, 35), Price = 110m, LotCount = 25, BuySell = BuySell.Sell }; // sell 25
            table.AddTrade(t);

            var positions = table.GetPositions();
            Assert.True(positions.Count() == 3); // стало 3 позиции

            var close_pos_20 = positions.FirstOrDefault(r => r.Count == 20 * ins.LotSize); // одна закрытая на 20

            Assert.Equal(dt2, close_pos_20.OpenTime);
            Assert.Equal(120, close_pos_20.OpenPrice);
            Assert.Equal(PosTypes.Long, close_pos_20.PosType);
            Assert.Equal(t.Time, close_pos_20.CloseTime);
            Assert.Equal(t.Price, close_pos_20.ClosePrice);
            Assert.True(close_pos_20.IsChanged);

            var open_pos_5 = positions.FirstOrDefault(r => r.Count == 5 * ins.LotSize && r.CloseTime == null); // одна открытая на 5

            Assert.Equal(dt1, open_pos_5.OpenTime);
            Assert.Equal(100, open_pos_5.OpenPrice);
            Assert.Equal(PosTypes.Long, open_pos_5.PosType);
            Assert.Null(open_pos_5.CloseTime);
            Assert.Null(open_pos_5.ClosePrice);
            Assert.False(open_pos_5.IsChanged); // это новая позиция, она не помечена как измененная

            var close_pos_5 = positions.FirstOrDefault(r => r.Count == 5 * ins.LotSize && r.CloseTime != null); // одна закрытая на 5

            Assert.Equal(dt1, close_pos_5.OpenTime);
            Assert.Equal(100, close_pos_5.OpenPrice);
            Assert.Equal(PosTypes.Long, close_pos_5.PosType);
            Assert.Equal(t.Time, close_pos_5.CloseTime);
            Assert.Equal(t.Price, close_pos_5.ClosePrice);
            Assert.True(close_pos_5.IsChanged);
        }

        [Fact]
        public void SetCount_pos_change()
        {
            PosTable table = new PosTable(instrumBL);
            var ins = instrumBL.GetInstrumByID(1);
            Position p = table.NewPosition(ins.InsID, 10, DateTime.Today, 100, PosTypes.Long);
            Assert.Equal(10, p.Count);

            Assert.False(p.IsChanged);

            p.SetCount(20);

            Assert.Equal(20, p.Count);
            Assert.True(p.IsChanged);
        }

        [Fact]
        public void ClosePosition_pos_changedPos()
        {
            PosTable table = new PosTable(instrumBL);
            var ins = instrumBL.GetInstrumByID(1);
            Position p = table.NewPosition(ins.InsID, 10, DateTime.Today, 100, PosTypes.Long);
            Assert.False(p.IsChanged);
            Assert.Null(p.ClosePrice);
            Assert.Null(p.CloseTime);

            DateTime ct = new DateTime(2010, 1, 1, 1, 1, 1);
            decimal cp = 200m;

            p.ClosePosition(ct, cp);

            Assert.True(p.IsChanged);
            Assert.Equal(ct, p.CloseTime);
            Assert.Equal(cp, p.ClosePrice);
        }

        /// <summary>
        /// Первоначально posTrades не было
        /// Открыли позицию - появилась одна posTrade
        /// Закрыли позицию - появилась вторая posTrade
        /// </summary>
        [Fact]
        public void AddTrade_noPos_twoPosTrades()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            Trade t1 = new Trade() { TradeID = 1, InsID = ins.InsID, OrderID = 2, Time = new DateTime(2012, 1, 1, 10, 30, 35), Price = 110m, LotCount = 25, BuySell = BuySell.Buy };
            table.AddTrade(t1);

            var positions1 = table.GetPositions().ToList();
            var posTrades1 = table.GetPosTrades().ToList();
            Assert.Single(positions1);
            Assert.Single(posTrades1);
            Assert.True(posTrades1[0].TradeID == t1.TradeID && posTrades1[0].PosID == positions1[0].PosID);

            Trade t2 = new Trade() { TradeID = 2, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 1, 10, 30, 40), Price = 100m, LotCount = 25, BuySell = BuySell.Sell };
            table.AddTrade(t2);
            var positions2 = table.GetPositions().ToList();
            var posTrades2 = table.GetPosTrades().ToList();

            Assert.Single(positions2);
            Assert.True(posTrades2.Count == 2);
            Assert.Single(posTrades2.Where(r => r.TradeID == t1.TradeID && r.PosID == positions1[0].PosID));
            Assert.Single(posTrades2.Where(r => r.TradeID == t2.TradeID && r.PosID == positions1[0].PosID));
        }

        /// <summary>
        /// Первоначально posTrades не было
        /// Открыли позицию
        /// Частично закрыли позицию
        /// </summary>
        [Fact]
        public void AddTrade_noPos_splitPosTrades()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            Trade t1 = new Trade() { TradeID = 1, InsID = ins.InsID, OrderID = 2, Time = new DateTime(2012, 1, 1, 10, 30, 35), Price = 110m, LotCount = 25, BuySell = BuySell.Buy };
            table.AddTrade(t1);
            Trade t2 = new Trade() { TradeID = 2, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 1, 10, 30, 40), Price = 100m, LotCount = 15, BuySell = BuySell.Sell };
            table.AddTrade(t2);

            var positions = table.GetPositions().ToList();
            var posTrades = table.GetPosTrades().ToList();
            Assert.True(posTrades.Count == 3);
            Assert.Single(posTrades.Where(r => r.TradeID == t1.TradeID && r.PosID == positions[0].PosID));
            Assert.Single(posTrades.Where(r => r.TradeID == t1.TradeID && r.PosID == positions[1].PosID));
            Assert.Single(posTrades.Where(r => r.TradeID == t2.TradeID && r.PosID == positions[0].PosID));
        }

        /// <summary>
        /// Первоначально posTrades не было
        /// Открыли позицию
        /// Закрыли позицию с большим кол-вом
        /// </summary>
        [Fact]
        public void AddTrade_noPos_newPosTrades()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            Trade t1 = new Trade() { TradeID = 1, InsID = ins.InsID, OrderID = 2, Time = new DateTime(2012, 1, 1, 10, 30, 35), Price = 110m, LotCount = 25, BuySell = BuySell.Buy };
            table.AddTrade(t1);
            Trade t2 = new Trade() { TradeID = 2, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 1, 10, 30, 40), Price = 100m, LotCount = 35, BuySell = BuySell.Sell };
            table.AddTrade(t2);

            var positions = table.GetPositions().ToList();
            var posTrades = table.GetPosTrades().ToList();
            Assert.True(posTrades.Count == 3);
            Assert.Single(posTrades.Where(r => r.TradeID == t1.TradeID && r.PosID == positions[0].PosID));
            Assert.Single(posTrades.Where(r => r.TradeID == t2.TradeID && r.PosID == positions[0].PosID));
            Assert.Single(posTrades.Where(r => r.TradeID == t2.TradeID && r.PosID == positions[1].PosID));
        }

        /// <summary>
        /// Есть несколько открытых позиций, которые подходят для закрытия.
        /// Нужно выбрать правильную открытую позицию для закрытия
        /// в соответствии с алгоритмом.
        /// </summary>
        [Fact]
        public void AddTrade_manyPos_closeCorrectPos()
        {
            var ins = instrumBL.GetInstrumByID(1);
            PosTable table = new PosTable(instrumBL);

            // инициализируем начальные позиции
            // 5 10 10 15
            Trade b1 = new Trade() { TradeID = 1, InsID = ins.InsID, OrderID = 2, Time = new DateTime(2012, 1, 1, 10, 0, 0), Price = 110m, LotCount = 5, BuySell = BuySell.Buy };
            table.AddTrade(b1);

            Trade b2 = new Trade() { TradeID = 2, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 2, 10, 0, 0), Price = 100m, LotCount = 10, BuySell = BuySell.Buy };
            table.AddTrade(b2);

            Trade b3 = new Trade() { TradeID = 3, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 3, 10, 0, 0), Price = 100m, LotCount = 10, BuySell = BuySell.Buy };
            table.AddTrade(b3);

            Trade b4 = new Trade() { TradeID = 4, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 4, 10, 0, 0), Price = 100m, LotCount = 15, BuySell = BuySell.Buy };
            table.AddTrade(b4);

            Assert.Equal(4, table.GetPositions().Where(p => p.CloseTime == null).Count()); // у нас 4 отктытых позиции

            // продаем 10, закрыть должны третью позицию, а не вторую
            Trade s1 = new Trade() { TradeID = 5, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 5, 10, 0, 0), Price = 100m, LotCount = 10, BuySell = BuySell.Sell };
            table.AddTrade(s1);

            Assert.Equal(3, table.GetPositions().Where(p => p.CloseTime == null).Count()); // теперь открытых только 3
            Assert.Single(table.GetPositions().Where(p => p.CloseTime != null)); // одна закрытая
            Assert.Equal(new DateTime(2012, 1, 3, 10, 0, 0), table.GetPositions().Where(p => p.CloseTime != null).First().OpenTime); // именно эта закрытая

            // продаем еще 12, закрыть должны последнюю позицию
            Trade s2 = new Trade() { TradeID = 6, InsID = ins.InsID, OrderID = 3, Time = new DateTime(2012, 1, 6, 10, 0, 0), Price = 100m, LotCount = 12, BuySell = BuySell.Sell };
            table.AddTrade(s2);

            var openPos = table.GetPositions().Where(p => p.CloseTime == null).ToList();
            Assert.Equal(3, openPos.Count); // открытых 3
            Assert.Equal(5, openPos[0].Count); // первая колик 5
            Assert.Equal(10, openPos[1].Count); // вторая колич 10
            Assert.Equal(3, openPos[2].Count); // третья колич 3
        }
    }
}
