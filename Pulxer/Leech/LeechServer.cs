using LeechPipe;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class LeechServer
    {
        private LpCore _core;
        private SystemLp _sysPipe;
        private ILpFactory _pipeFactory;
        private readonly InstrumCache _instrumCache;
        private readonly ILogger _logger;

        public LeechServer(LeechPipeServerSocket socket, InstrumCache instrumCache, ILogger logger)
        {
            _logger = logger;
            _instrumCache = instrumCache;
            _core = new LpCore(socket, true); // сервер
            _pipeFactory = new LeechServerPipeFactory(_core);
            _sysPipe = new SystemLp(_pipeFactory, _core);
            _core.Initialize(_sysPipe, "Pulxer", false);
        }

        public async Task Run()
        {
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
            var syncPipe = await _sysPipe.CreatePipeAsync(Encoding.UTF8.GetBytes("sync"));
            if (syncPipe == 0) return null;

            return new SyncPipeServer(_core, syncPipe);
        }

        public async Task<TickPipeServer> CreateTickPipe()
        {
            var tickPipe = await _sysPipe.CreatePipeAsync(Encoding.UTF8.GetBytes("tick"));
            if (tickPipe == 0) return null;

            return new TickPipeServer(_core, tickPipe, _instrumCache, _logger);
        }

        public async Task<bool> DeletePipe(ushort pipe)
        {
            if (pipe == 0) return false;

            return await _sysPipe.DeletePipeAsync(pipe);
        }
    }
}
