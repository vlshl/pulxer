using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Storage.Test
{
    public class PositionTest
    {
        private readonly DbContextOptions<DaContext> _options;

        private readonly IInstrumDA _insDA;
        private readonly IAccountDA _accountDA;
        private readonly IPositionDA _positionDA;
        private int _gazpID;
        private int _lkohID;
        private int _accountID;

        public PositionTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;

            _insDA = new InstrumDA(_options);
            _accountDA = new AccountDA(_options);
            _positionDA = new PositionDA(_options);

            // создание
            var gazp = _insDA.GetInstrum(0, "GAZP");
            if (gazp == null)
            {
                _gazpID = _insDA.InsertInstrum("GAZP", "Газпром", "Газпром", 10, 2, 1);
            }
            else
            {
                _gazpID = gazp.InsID;
            }

            var lkoh = _insDA.GetInstrum(0, "LKOH");
            if (lkoh == null)
            {
                _lkohID = _insDA.InsertInstrum("LKOH", "Лукойл", "Лукойл", 1, 0, 0);
            }
            else
            {
                _lkohID = lkoh.InsID;
            }

            _accountID = _accountDA.CreateAccount("", "", 0, false, Common.Data.AccountTypes.Test).AccountID;
        }

        [Fact]
        public void CreateGetUpdatePosition()
        {
            DateTime od = new DateTime(2010, 2, 17, 8, 30, 0);
            DateTime cd = new DateTime(2010, 5, 7, 18, 20, 15);
            var openPos = _positionDA.CreatePosition(_accountID, _gazpID, Common.Data.PosTypes.Long, od, 100, 10, null, null);
            var closePos = _positionDA.CreatePosition(_accountID, _lkohID, Common.Data.PosTypes.Short, cd, 1000, 7, DateTime.Now, 1100);

            var allPosList = _positionDA.GetPositions(_accountID, false);
            var openPosList = _positionDA.GetPositions(_accountID, true);

            if (allPosList.Count() != 2) throw new Exception();
            if (openPosList.Count() != 1) throw new Exception();

            openPos.ClosePosition(cd, 100); // закрыли
            _positionDA.UpdatePosition(openPos); // и обновили

            var openPosList1 = _positionDA.GetPositions(_accountID, true); // извлекаем открытую
            if (openPosList1.Any()) throw new Exception(); // а их уже нет
        }

        [Fact]
        public void PosTrades()
        {
            var order = _accountDA.CreateOrder(_accountID, DateTime.Today, _gazpID, Platform.BuySell.Buy, 1, null, Platform.OrderStatus.Active, null, 0);
            var trade = _accountDA.CreateTrade(_accountID, order.OrderID, DateTime.Today, _gazpID, Platform.BuySell.Buy, 1, 100, 0, 0);

            var newTrades = _positionDA.GetNewTrades(_accountID);
            if (!newTrades.Any(r => r.TradeID == trade.TradeID)) throw new Exception(); // должен быть такой trade

            var openPos = _positionDA.CreatePosition(_accountID, _gazpID, Common.Data.PosTypes.Long, DateTime.Today, 100, 10, null, null);
            var closePos = _positionDA.CreatePosition(_accountID, _gazpID, Common.Data.PosTypes.Long, DateTime.Today, 100, 10, DateTime.Today, 100);
            _positionDA.AddPosTrade(openPos.PosID, trade.TradeID);
            _positionDA.AddPosTrade(closePos.PosID, trade.TradeID);
            var posTrades = _positionDA.GetOpenPosTrades(_accountID); // только открытые
            int c1 = posTrades.Count(r => r.PosID == openPos.PosID && r.TradeID == trade.TradeID); // =1
            int c2 = posTrades.Count(r => r.PosID == closePos.PosID && r.TradeID == trade.TradeID); // =0
            if (c1 != 1) throw new Exception();
            if (c2 != 0) throw new Exception();

            newTrades = _positionDA.GetNewTrades(_accountID);
            if (newTrades.Any(r => r.TradeID == trade.TradeID)) throw new Exception(); // не должно быть такого trade, поскольку есть записи posTrades
        }
    }
}
