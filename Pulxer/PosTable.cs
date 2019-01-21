using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulxer
{
    public class PosTable
    {
        private readonly IInstrumBL _instrumBL = null;
        private List<Position> _positions = null;
        private List<PosTrade> _posTrades = null;
        private int _accountID = 0;

        public PosTable(IInstrumBL instrumBL)
        {
            _instrumBL = instrumBL;
            _positions = new List<Position>();
            _posTrades = new List<PosTrade>();
        }

        /// <summary>
        /// Загрузка позиций и связей со сделками
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        /// <param name="positions">Только открытые позиции</param>
        /// <param name="posTrades">Связи только по открытым позициям</param>
        public void Initialize(int accountID, IEnumerable<Position> positions, IEnumerable<PosTrade> posTrades)
        {
            _accountID = accountID;
            _positions.Clear();
            _posTrades.Clear();
            if (positions != null) _positions.AddRange(positions);
            if (posTrades != null) _posTrades.AddRange(posTrades);
        }

        public IEnumerable<Position> GetPositions()
        {
            return _positions.OrderBy(r => r.OpenTime).ToList();
        }

        public IEnumerable<PosTrade> GetPosTrades()
        {
            return _posTrades.ToList();
        }

        public void AddTrade(Trade trade)
        {
            var instrum = _instrumBL.GetInstrumByID(trade.InsID);
            if (instrum == null) return;

            PosTypes pt = trade.BuySell == BuySell.Buy ? PosTypes.Short : PosTypes.Long;
            int count = trade.LotCount * instrum.LotSize;

            var openPos = from p in _positions
                          let srt = p.Count == count ? 0 : ( p.Count > count ? 1 : 2 )
                          where p.InsID == trade.InsID && p.CloseTime == null && p.PosType == pt
                          orderby srt ascending, p.OpenTime descending
                          select p;

            foreach (var p in openPos)
            {
                if (count <= 0) break;

                if (count < p.Count)
                {
                    SplitPosition(p, count);
                    p.ClosePosition(trade.Time, trade.Price);
                    AddPosTrade(p.PosID, trade.TradeID);
                    count = 0;
                    break;
                }
                else if (count == p.Count)
                {
                    p.ClosePosition(trade.Time, trade.Price);
                    AddPosTrade(p.PosID, trade.TradeID);
                    count = 0;
                    break;
                }
                else
                {
                    p.ClosePosition(trade.Time, trade.Price);
                    AddPosTrade(p.PosID, trade.TradeID);
                    count -= p.Count;
                }
            }
            if (count > 0)
            {
                PosTypes pt1 = trade.BuySell == BuySell.Buy ? PosTypes.Long : PosTypes.Short;
                Position pos1 = AddPosition(trade.InsID, count, trade.Time, trade.Price, pt1);
                AddPosTrade(pos1.PosID, trade.TradeID);
            }
        }

        private Position SplitPosition(Position pos, int count)
        {
            int posCount = pos.Count;
            if (count >= posCount) return null; // не нужно делать сплит

            pos.SetCount(count);
            int rest = posCount - count;

            Position pos1 = AddPosition(pos.InsID, rest, pos.OpenTime, pos.OpenPrice, pos.PosType);
            var posTrade = _posTrades.FirstOrDefault(r => r.PosID == pos.PosID);
            if (posTrade != null)
            {
                AddPosTrade(pos1.PosID, posTrade.TradeID);
            }

            return pos1;
        }

        private Position AddPosition(int insID, int count, DateTime openTime, decimal openPrice, PosTypes pt)
        {
            Position pos = NewPosition(insID, count, openTime, openPrice, pt);
            _positions.Add(pos);
            return pos;
        }

        private void AddPosTrade(int posID, int tradeID)
        {
            PosTrade pt = new PosTrade(posID, tradeID);
            pt.IsNew = true;
            _posTrades.Add(pt);
        }

        public Position NewPosition(int insID, int count, DateTime openTime, decimal openPrice, PosTypes posType)
        {
            return new Position()
            {
                PosID = --_posID,
                AccountID = _accountID,
                InsID = insID,
                Count = count,
                OpenTime = openTime,
                OpenPrice = openPrice,
                PosType = posType,
                CloseTime = null,
                ClosePrice = null,
                IsChanged = false
            };
        }
        private int _posID = 0;
    }
}
