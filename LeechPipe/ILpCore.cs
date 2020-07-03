using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LeechPipe
{
    public interface ILpCore
    {
        Task<byte[]> SendMessageAsync(ushort pipe, byte[] data);
        Task SendResponseAsync(ILpReceiver handler, byte[] data);
    }
}
