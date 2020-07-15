using System;
using System.Collections.Generic;
using System.Text;

namespace LeechPipe
{
    public interface ILpReceiver
    {
        void OnRecv(byte[] data);
    }
}
