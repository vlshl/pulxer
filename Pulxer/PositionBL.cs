using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Pulxer
{
    public interface IPositionBL
    {
        PosTable GetPosTable(int accountID);
        void SavePosTable(PosTable table);
        void RefreshPositions(int accountID);
        IEnumerable<Position> GetOpenedPositions(int accountID);
        IEnumerable<Position> GetClosedPositions(int accountID);
        IEnumerable<Position> GetAllPositions(int accountID);
        IEnumerable<Position> GetPositions(IEnumerable<int> ids);
        IEnumerable<PosTrade> GetPosTrades(IEnumerable<int> posIDs);
        void ClearPositions(int accountID);
    }

    public class PositionBL : IPositionBL
    {
        private readonly IPositionDA _positionDA;
        private readonly IInstrumBL _instrumBL = null;

        public PositionBL(IPositionDA positionDA, IInstrumBL instrumBL)
        {
            _positionDA = positionDA;
            _instrumBL = instrumBL;
        }

        /// <summary>
        /// Получить объект PosTable - таблица позиций
        /// </summary>
        /// <param name="accountID">Account ID</param>
        /// <returns>Объект PosTable</returns>
        public PosTable GetPosTable(int accountID)
        {
            var positions = _positionDA.GetPositions(accountID, true);
            var posTrades = _positionDA.GetOpenPosTrades(accountID);
            if (positions == null) positions = new List<Position>();
            if (posTrades == null) posTrades = new List<PosTrade>();

            PosTable table = new PosTable(_instrumBL);
            table.Initialize(accountID, positions, posTrades);

            return table;
        }

        /// <summary>
        /// Сохранить таблицу позиций в базе,
        /// также сохраняется таблица связей Позиция-Сделка
        /// </summary>
        /// <param name="table">Таблица позиций</param>
        public void SavePosTable(PosTable table)
        {
            var positions = table.GetPositions();
            var posTrades = table.GetPosTrades();

            foreach (var pos in positions)
            {
                if (pos.PosID <= 0)
                {
                    var newPos = _positionDA.CreatePosition(pos.AccountID, pos.InsID, pos.PosType, pos.OpenTime, pos.OpenPrice, pos.Count, pos.CloseTime, pos.ClosePrice);
                    posTrades.Where(r => r.PosID == pos.PosID).ToList()
                        .ForEach(r => r.PosID = newPos.PosID);
                    pos.PosID = newPos.PosID;
                }
                else if (pos.IsChanged)
                {
                    _positionDA.UpdatePosition(pos);
                }
            }

            foreach (var pt in posTrades)
            {
                if (pt.IsNew)
                {
                    _positionDA.AddPosTrade(pt.PosID, pt.TradeID);
                }
            }
        }

        /// <summary>
        /// Обновить таблицу позиций.
        /// Сначала получаем таблицу позиций из базы, 
        /// затем прогоняем через нее все новые сделки, 
        /// в конце сохраняем таблицу позиций в базе
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        public void RefreshPositions(int accountID)
        {
            var trades = _positionDA.GetNewTrades(accountID);
            if (!trades.Any()) return;

            var sortedTrades = (from t in trades orderby t.Time, t.TradeID select t).ToList();
            var table = GetPosTable(accountID);
            foreach (var trade in sortedTrades)
            {
                table.AddTrade(trade);
            }
            SavePosTable(table);
        }

        /// <summary>
        /// Очистить таблицу позиций для выбранного торгового счета.
        /// Все позиции удаляются, PosTrades тоже очищается.
        /// После этого RefreshPositions пересоздаст все заново
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        public void ClearPositions(int accountID)
        {
            _positionDA.DeleteAllPositions(accountID);
        }

        /// <summary>
        /// Получить список открытых позиций
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <returns></returns>
        public IEnumerable<Position> GetOpenedPositions(int accountID)
        {
            return _positionDA.GetPositions(accountID, true);
        }

        /// <summary>
        /// Получить список закрытых позиций
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <returns></returns>
        public IEnumerable<Position> GetClosedPositions(int accountID)
        {
            return _positionDA.GetPositions(accountID, false);
        }

        /// <summary>
        /// Получить список всех позиций
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <returns></returns>
        public IEnumerable<Position> GetAllPositions(int accountID)
        {
            return _positionDA.GetPositions(accountID, null);
        }

        /// <summary>
        /// Получить список позиций по указанным ID
        /// </summary>
        /// <param name="ids">Требуемые ID</param>
        /// <returns>Список позиций</returns>
        public IEnumerable<Position> GetPositions(IEnumerable<int> ids)
        {
            return _positionDA.GetPositions(ids);
        }

        /// <summary>
        /// Получить список сделок по позициям
        /// </summary>
        /// <param name="posIDs">Список позиций</param>
        /// <returns>Список сделок по позициям</returns>
        public IEnumerable<PosTrade> GetPosTrades(IEnumerable<int> posIDs)
        {
            return _positionDA.GetPosTrades(posIDs);
        }
    }
}
