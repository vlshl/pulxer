using System;
using System.Collections.Generic;
using System.Text;

namespace Storage.DbModel
{
    public class LeechStopOrder
    {
        public int StopOrderID { get; set; }
        public int Time { get; set; }
        public int InsID { get; set; }
        public int BuySell { get; set; }
        public int StopType { get; set; }
        public int? EndTime { get; set; }
        public decimal AlertPrice { get; set; }
        public decimal? Price { get; set; }
        public int LotCount { get; set; }
        public int Status { get; set; }
        public int AccountID { get; set; }
        public int? CompleteTime { get; set; }
        public long StopOrderNo { get; set; }
    }

    public class LeechOrder
    {
        public int OrderID { get; set; }
        public int Time { get; set; }
        public int InsID { get; set; }
        public int BuySell { get; set; }
        public int LotCount { get; set; }
        public decimal? Price { get; set; }
        public int Status { get; set; }
        public int AccountID { get; set; }
        public int? StopOrderID { get; set; }
        public long OrderNo { get; set; }
    }

    public class LeechTrade
    {
        public int TradeID { get; set; }
        public int OrderID { get; set; }
        public int Time { get; set; }
        public int InsID { get; set; }
        public int BuySell { get; set; }
        public int LotCount { get; set; }
        public decimal Price { get; set; }
        public int AccountID { get; set; }
        public decimal Comm { get; set; }
        public long TradeNo { get; set; }
    }

}
