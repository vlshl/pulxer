using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeechPipe
{
    public interface ILpTransport
    {
        Task SendMessageAsync(byte[] buffer, int offset, int count);
        Task<int> RecvMessageAsync(byte[] buffer);
    }
}
