using LeechPipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class LeechServer
    {
        private LpCore _core;
        private SystemLp _sysPipe;
        private ILpFactory _pipeFactory;
        private LeechPipeServerSocket _serverSocket;
        private ushort _syncPipe;
        private ushort _tickPipe;
        private string _account;
        private TickPipeServer _tickPipeServer;

        public LeechServer(string account)
        {
            _account = account;
            _tickPipeServer = null;
        }

        public async Task Run(WebSocket socket)
        {
            _serverSocket = new LeechPipeServerSocket(socket);
            _core = new LpCore(_serverSocket, true); // сервер
            _pipeFactory = new LeechServerPipeFactory(_core);
            _sysPipe = new SystemLp(_pipeFactory, _core);
            _core.Initialize(_sysPipe, "Pulxer", false);

            await _core.DoRecv();
        }

        public void Close()
        {
            _core.Close();
        }

        public async Task<string> GetRemoteIdentity()
        {
            return await _sysPipe.GetRemoteIdentity();
        }

        public async Task<SyncPipeServer> CreateSyncPipe()
        {
            _syncPipe = await _sysPipe.CreatePipeAsync(Encoding.UTF8.GetBytes("sync"));
            if (_syncPipe == 0) return null;

            return new SyncPipeServer(_core, _syncPipe);
        }

        public async Task<bool> DeleteSyncPipe()
        {
            if (_syncPipe == 0) return false;

            return await _sysPipe.DeletePipeAsync(_syncPipe);
        }

        public async Task<TickPipeServer> GetTickPipe()
        {
            if (_tickPipeServer != null) return _tickPipeServer;

            _tickPipe = await _sysPipe.CreatePipeAsync(Encoding.UTF8.GetBytes("tick"));
            if (_tickPipe == 0) return null;

            _tickPipeServer = new TickPipeServer(_core, _tickPipe);
            return _tickPipeServer;
        }

        // не вызывается никогда
        public async Task<bool> DeleteTickPipe()
        {
            if (_tickPipe == 0) return false;

            return await _sysPipe.DeletePipeAsync(_tickPipe);
        }
    }
}
