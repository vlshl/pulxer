using System;
using System.Collections.Generic;
using System.Text;

namespace LeechPipe
{
    public interface ILpFactory
    {
        ILpReceiver CreatePipe(byte[] pipeInitData);
    }
}
