using Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class OpenPosItem
    {
        public int PosId { get { return _pos.PosID; } }
        public int InsId { get { return _ins.InsID; } }
        public string Ticker { get { return _ins.Ticker; } }
        public string ShortName { get { return _ins.ShortName; } }
        public string Name { get { return _ins.Name; } }
        public int Count { get { return _pos.Count; } }
        public int Lots { get { return _lots; } }
        public decimal OpenPrice { get { return _pos.OpenPrice; } }
        public decimal OpenSumma { get { return _openSumma; } }
        public DateTime OpenTime { get { return _pos.OpenTime; } }
        public PosTypes PosType { get { return _pos.PosType; } }
        public decimal CurPrice { get { return _curPrice; } }
        public decimal CurSumma { get { return _curSumma; } }
        public decimal Profit { get { return _profit; } }
        public decimal ProfitPerc { get { return _profitPerc; } }

        private readonly Position _pos;
        private readonly Instrum _ins;
        private decimal _curPrice;
        private decimal _openSumma;
        private decimal _curSumma;
        private decimal _profit;
        private decimal _profitPerc;
        private int _lots;

        public void SetCurPrice(decimal price)
        {
            _curPrice = price;
            _curSumma = _curPrice * _pos.Count;
            _profit = _curSumma - _openSumma;
            _profitPerc = _openSumma > 0 ? _profit / _openSumma * 100 : 0;
        }

        public OpenPosItem(Position pos, Instrum ins)
        {
            _pos = pos;
            _ins = ins;
            _openSumma = _pos.OpenPrice * _pos.Count;
            _lots = _pos.Count / _ins.LotSize;
        }
    }
}
