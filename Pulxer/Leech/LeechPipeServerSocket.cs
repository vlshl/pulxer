using LeechPipe;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LeechPipeServerSocket> _logger;

        public LeechPipeServerSocket(WebSocket socket, ILogger<LeechPipeServerSocket> logger)
        {
            _socket = socket;
            _logger = logger;
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

            try
            {
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
            catch(Exception ex)
            {
                _logger.LogError(ex, "RecvMessageAsync: receive error");
                return null;
            }
        }

        public async Task<bool> SendMessageAsync(byte[] buffer)
        {
            int offset = 0;
            try
            {
                while (offset < buffer.Length)
                {
                    int restLen = buffer.Length - offset;
                    bool endOfMessage = restLen <= LpCore.SEGMENT_SIZE;
                    int count = endOfMessage ? restLen : LpCore.SEGMENT_SIZE;

                    await _socket.SendAsync(new ArraySegment<byte>(buffer, offset, count),
                        WebSocketMessageType.Binary, endOfMessage, _cts.Token);

                    offset += count;
                }

                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "SendMessageAsync: send error");
                return false;
            }
        }
    }
}
