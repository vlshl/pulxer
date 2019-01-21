using Common.Data;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Text;

namespace PulxerTest
{
    public class ReplicationBLMock : IReplicationBL
    {
        private Dictionary<ReplObjects, Dictionary<int, int>> _obj_repls;

        public ReplicationBLMock()
        {
            _obj_repls = new Dictionary<ReplObjects, Dictionary<int, int>>();
        }

        public Dictionary<int, int> GetReplications(ReplObjects ro)
        {
            if (!_obj_repls.ContainsKey(ro))
            {
                _obj_repls.Add(ro, new Dictionary<int, int>());
            }
            return _obj_repls[ro];
        }

        public void UpdateReplications(ReplObjects ro, Dictionary<int, int> rid_lid)
        {
            if (_obj_repls.ContainsKey(ro)) _obj_repls.Remove(ro);
            _obj_repls.Add(ro, rid_lid);
        }
    }
}
