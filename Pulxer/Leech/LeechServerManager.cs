using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class LeechServerManager
    {
        private LeechServer _leechServer = null;

        public LeechServerManager()
        {
        }

        public LeechServer CreateServer(LeechPipeServerSocket socket)
        {
            lock (this)
            {
                if (_leechServer != null) return null;

                _leechServer = new LeechServer(socket);
                return _leechServer;
            }
        }

        public void DeleteServer()
        {
            _leechServer = null;
        }

        public LeechServer GetServer()
        {
            lock (this)
            {
                return _leechServer;
            }
        }
    }
}
