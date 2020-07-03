using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class LeechServerManager
    {
        private Dictionary<string, LeechServer> _acc_servers;

        public LeechServerManager()
        {
            _acc_servers = new Dictionary<string, LeechServer>();
        }

        public LeechServer CreateServer(string account)
        {
            lock (_acc_servers)
            {
                var ls = new LeechServer(account);
                _acc_servers.Add(account, ls);
                return ls;
            }
        }

        public void DeleteServer(string account)
        {
            lock (_acc_servers)
            {
                _acc_servers.Remove(account);
            }
        }

        //public LeechServer[] GetLeechServers()
        //{
        //    lock (_acc_servers)
        //    {
        //        return _acc_servers.ToArray();
        //    }
        //}
    }
}
