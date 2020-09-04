using Common.Data;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IPositionDA
    {
        IEnumerable<Position> GetPositions(int accountID, bool? isOpened);
        IEnumerable<Position> GetPositions(IEnumerable<int> ids);
        Position CreatePosition(int accountID, int insID, PosTypes posType, DateTime openTime, decimal openPrice, int count, DateTime? closeTime, decimal? closePrice);
        void UpdatePosition(Position pos);
        IEnumerable<PosTrade> GetOpenPosTrades(int accountID);
        void AddPosTrade(int posID, int tradeID);
        IEnumerable<Trade> GetNewTrades(int accountID);
        IEnumerable<PosTrade> GetPosTrades(IEnumerable<int> posIDs);
        void DeleteAllPositions(int accountID);
    }
}
