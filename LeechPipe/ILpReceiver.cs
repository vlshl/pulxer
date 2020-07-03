using System;
using System.Collections.Generic;
using System.Text;

namespace LeechPipe
{
    public interface ILpReceiver
    {
        public void OnRecv(byte[] data);
    }
}
