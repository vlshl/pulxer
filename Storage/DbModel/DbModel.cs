using Common;
using Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Storage.DbModel
{
    public class InsStorePeriods
    {
        public int InsStoreID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLastDirty { get; set; }
    }

    public class InsStoreFreeDays
    {
        public int InsStoreID { get; set; }
        public DateTime Date { get; set; }
    }

    public class DbChart
    {
        public int ChartID { get; set; }
        public int InsID { get; set; }
        public byte Tf { get; set; }
        public int AccountID { get; set; }
        public string Data { get; set; }
    }

}
