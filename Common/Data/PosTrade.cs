using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    public class PosTrade
    {
        public int PosID { get; set; }
        public int TradeID { get; set; }
        public bool IsNew { get; set; }

        public PosTrade(int posID, int tradeID)
        {
            PosID = posID;
            TradeID = tradeID;
            IsNew = false;
        }
    }
}
