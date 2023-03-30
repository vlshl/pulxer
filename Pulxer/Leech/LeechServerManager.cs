using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class LeechServerManager
    {
        private LeechServer _leechServer = null;
        private readonly InstrumCache _instrumCache;
        private readonly ILogger<LeechServerManager> _logger;

        public LeechServerManager(InstrumCache instrumCache, ILogger<LeechServerManager> logger)
        {
            _instrumCache = instrumCache;
            _logger = logger;
        }

        public bool CreateServer(LeechPipeServerSocket socket)
        {
            lock (this)
            {
                if (_leechServer != null) return false;

                _leechServer = new LeechServer(socket, _instrumCache, _logger);
                return true;
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
