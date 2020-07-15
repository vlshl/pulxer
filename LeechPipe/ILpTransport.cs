using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeechPipe
{
    public interface ILpTransport
    {
        Task SendMessageAsync(byte[] buffer);
        Task<byte[]> RecvMessageAsync();
    }
}
