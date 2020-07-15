using LeechPipe;
using System;
using System.Collections.Generic;
using System.IO;
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

        public async Task<byte[]> RecvMessageAsync()
        {
            byte[] buffer = new byte[LpCore.SEGMENT_SIZE];
            WebSocketReceiveResult res;

            using (var ms = new MemoryStream())
            {
                do
                {
                    ArraySegment<byte> segm = new ArraySegment<byte>(buffer, 0, buffer.Length);
                    res = await _socket.ReceiveAsync(segm, _cts.Token);
                    ms.Write(buffer, 0, res.Count);
                } while (!res.EndOfMessage);

                return ms.ToArray();
            }
        }

        public async Task SendMessageAsync(byte[] buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int restLen = buffer.Length - offset;
                bool endOfMessage = restLen <= LpCore.SEGMENT_SIZE;
                int count = endOfMessage ? restLen : LpCore.SEGMENT_SIZE;

                await _socket.SendAsync(new ArraySegment<byte>(buffer, offset, count),
                    WebSocketMessageType.Binary, endOfMessage, _cts.Token);

                offset += count;
            }
        }
    }
}
