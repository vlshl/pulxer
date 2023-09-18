using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class TradePosition
    {
        public int PosID { get; set; }
        public int InsID { get; set; }
        public int Count { get; set; } // кол-во бумаг (не лотов)
        public DateTime OpenTime { get; set; }
        public decimal OpenPrice { get; set; }
        public DateTime? CloseTime { get; set; }
        public decimal? ClosePrice { get; set; }
        public byte PosType { get; set; }
        public int AccountID { get; set; }
        public IList<int> TradeIDs { get; set; }
    }
}
