using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer
{
    public class AccountMeta
    {
        public DateTime TickSource_StartDate { get; set; }
        public DateTime TickSource_EndDate { get; set; }
        public string TickSource_Tickers { get; set; }
        public DateTime TestRun_CompleteTime { get; set; }
    }
}
