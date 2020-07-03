using LeechPipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class LeechServerPipeFactory : ILpFactory
    {
        private ILpCore _core;

        public LeechServerPipeFactory(ILpCore core)
        {
            _core = core;
        }

        public ILpReceiver CreatePipe(byte[] pipeInitData)
        {
            return null;
        }
    }
}
