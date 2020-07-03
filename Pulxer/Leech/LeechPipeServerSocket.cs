using LeechPipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class LeechPipeServerSocket : ILpTransport
    {
        private WebSocket _socket;
        private CancellationTokenSource _cts;

        public LeechPipeServerSocket(WebSocket socket)
        {
            _socket = socket;
            _cts = new CancellationTokenSource();
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

        public bool IsSocketOpen
        {
            get
            {
                return _socket.State == WebSocketState.Open;
            }
        }

        public async Task<int> RecvMessageAsync(byte[] buffer)
        {
            WebSocketReceiveResult res;
            int offset = 0; int count = 0;

            try
            {
                do
                {
                    ArraySegment<byte> segm = new ArraySegment<byte>(buffer, offset, buffer.Length - offset);
                    res = await _socket.ReceiveAsync(segm, _cts.Token);
                    offset += res.Count;
                    count += res.Count;
                    if (offset >= buffer.Length) offset = 0;
                } while (!res.EndOfMessage);
            }
            catch(Exception ex)
            {
                return 0;
            }

            if (count > buffer.Length) return 0;

            return count;
        }

        public async Task SendMessageAsync(byte[] buffer, int offset, int count)
        {
            await _socket.SendAsync(new ArraySegment<byte>(buffer, offset, count),
                WebSocketMessageType.Binary, true, _cts.Token);
        }
    }
}
