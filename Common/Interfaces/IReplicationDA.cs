using Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IReplicationDA
    {
        IEnumerable<Replication> GetReplications(ReplObjects objType);
        void InsertReplications(ReplObjects replObject, Dictionary<int, int> rid_lid);
        void DeleteReplications(ReplObjects replObject);
    }
}
