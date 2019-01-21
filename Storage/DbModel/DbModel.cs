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
}
