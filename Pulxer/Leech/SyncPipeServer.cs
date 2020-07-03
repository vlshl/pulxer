using LeechPipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class SyncPipeServer
    {
        private ILpCore _core;
        private ushort _pipe;

        public SyncPipeServer(ILpCore core, ushort pipe)
        {
            _core = core;
            _pipe = pipe;
        }

        public async Task<byte[]> SendCommand1()
        {
            return await _core.SendMessageAsync(_pipe, new byte[] { 0x01 });
        }

        public async Task<byte[]> SendCommand2()
        {
            return await _core.SendMessageAsync(_pipe, new byte[] { 0x02 });
        }
    }
}
