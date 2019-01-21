using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulxerTest
{
    public class PositionDAMock : IPositionDA
    {
        private List<PosTrade> _posTrades;
        private List<Position> _positions;
        private int _posID = 1;

        public PositionDAMock()
        {
            _posTrades = new List<PosTrade>();
            _positions = new List<Position>();
        }

        public void AddPosTrade(int posID, int tradeID)
        {
            if (_posTrades.Any(r => r.PosID == posID && r.TradeID == tradeID))
                throw new Exception("dublicate");
            PosTrade pt = new PosTrade(posID, tradeID);
            _posTrades.Add(pt);
        }

        public Position CreatePosition(int accountID, int insID, PosTypes posType, DateTime openTime, decimal openPrice, int count, DateTime? closeTime, decimal? closePrice)
        {
            Position p = new Position()
            {
                PosID = _posID++,
                AccountID = accountID,
                InsID = insID,
                PosType = posType,
                OpenTime = openTime,
                OpenPrice = openPrice,
                Count = count,
                CloseTime = closeTime,
                ClosePrice = closePrice
            };
            _positions.Add(p);

            return p;
        }

        public void DeleteAllPositions(int accountID)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<Trade> GetNewTrades(int accountID)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PosTrade> GetOpenPosTrades(int accountID)
        {
            var posIDs = GetPositions(accountID, true).Select(r => r.PosID);
            return _posTrades.Where(r => posIDs.Contains(r.PosID));
        }

        public IEnumerable<Position> GetPositions(int accountID, bool isOpenOnly)
        {
            if (isOpenOnly)
                return _positions.Where(r => r.AccountID == accountID && r.CloseTime == null);
            else
                return _positions.Where(r => r.AccountID == accountID);
        }

        public IEnumerable<Position> GetPositions(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<PosTrade> GetPosTrades(IEnumerable<int> posIDs)
        {
            throw new NotImplementedException();
        }

        public void UpdatePosition(Position pos)
        {
            if (pos.PosID <= 0) throw new Exception("New position");
            var found = _positions.FirstOrDefault(r => r.PosID == pos.PosID);
            if (found == null) throw new Exception("Not found");
            _positions.Remove(found);
            _positions.Add(pos);
        }
    }
}
