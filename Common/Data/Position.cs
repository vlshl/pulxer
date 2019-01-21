using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    public class Position
    {
        public int PosID { get; set; }
        public int InsID { get; set; }
        public int Count { get; set; } // кол-во бумаг (не лотов)
        public DateTime OpenTime { get; set; }
        public decimal OpenPrice { get; set; }
        public DateTime? CloseTime { get; set; }
        public decimal? ClosePrice { get; set; }
        public PosTypes PosType { get; set; }
        public int AccountID { get; set; }
        public bool IsChanged { get; set; }

        public Position()
        {
            PosID = 0;
            InsID = 0;
            Count = 0;
            OpenTime = DateTime.MinValue;
            OpenPrice = 0;
            CloseTime = null;
            ClosePrice = null;
            PosType = PosTypes.Long;
            AccountID = 0;
            IsChanged = false;
        }

        public void ClosePosition(DateTime closeTime, decimal closePrice)
        {
            CloseTime = closeTime;
            ClosePrice = closePrice;
            IsChanged = true;
        }

        public void SetCount(int count)
        {
            Count = count;
            IsChanged = true;
        }
    }

    public enum PosTypes
    {
        Long = 0,
        Short = 1
    }
}
