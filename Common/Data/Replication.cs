using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    public class Replication
    {
        public int LocalID { get; set; }
        public int RemoteID { get; set; }
        public ReplObjects ReplObject { get; set; }
    }

    public enum ReplObjects
    {
        Instrum = 1,
        Account = 2,
        StopOrder = 3,
        Order = 4,
        Trade = 5
    }
}
