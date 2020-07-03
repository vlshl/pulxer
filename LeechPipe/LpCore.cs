using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LeechPipe
{
    /// <summary>
    /// Работа с потоками на низком уровне.
    /// Прием данных: парсинг стрима входных данных, распределение сообщений по очередям и оповещение
    /// потребителей о добавлении нового сообщения в очередь.
    /// Отправка данных: Постановка нового сообщения в очередь на отправку и отправка отдельным потоком.
    /// </summary>
    public class LpCore : ILpCore
    {
        private const int TIMEOUT = 5000;
        private ILpTransport _transport;
        private bool _isWorking;
        private const int BUFFER_SIZE = 4 * 1024;
        private byte[] _recvBuffer;
        private Dictionary<ushort, ConcurrentQueue<byte[]>> _recvMessages; // очереди принятых сообщений по потокам
        private AutoResetEvent _recvAre;
        private Thread _recvThread;
        private Thread _recvThread1;
        private Dictionary<ushort, ILpReceiver> _pipe_recvs; // обработчики по потокам
        private bool _isServer;

        private Dictionary<ushort, WaitRecvItem> _waitItems;
        private string _identity = "";

        public LpCore(ILpTransport transport, bool isServer)
        {
            _transport = transport;
            _isWorking = true;
            _recvBuffer = new byte[BUFFER_SIZE];
            _recvMessages = new Dictionary<ushort, ConcurrentQueue<byte[]>>();
            _recvAre = new AutoResetEvent(false);
            _pipe_recvs = new Dictionary<ushort, ILpReceiver>();
            _isServer = isServer;
            _waitItems = new Dictionary<ushort, WaitRecvItem>();
        }

        public void Initialize(SystemLp sysPipe, string identity, bool createRecvThread = true)
        {
            lock (this)
            {
                _identity = identity;
                _pipe_recvs.Clear();
                _pipe_recvs.Add(0, sysPipe);

                _recvMessages.Clear();
                _recvMessages.Add(0, new ConcurrentQueue<byte[]>()); // нулевой пайп

                _waitItems.Clear();

                if (createRecvThread)
                {
                    _recvThread = new Thread(new ThreadStart(async () => { await DoRecv(); }));
                    _recvThread.Start();
                }

                _recvThread1 = new Thread(new ThreadStart(DoRecv1));
                _recvThread1.Start();
            }
        }

        public string GetIdentity()
        {
            return _identity;
        }

        /// <summary>
        /// Создание нового пайпа с присвоением ему номера
        /// </summary>
        /// <param name="pipeHandler"></param>
        /// <returns></returns>
        public ushort CreatePipe(ILpReceiver pipeHandler)
        {
            lock (this)
            {
                ushort pipe = FindFreeNumber();
                if (pipe == 0) return 0; // нет свободного номера

                _pipe_recvs.Add(pipe, pipeHandler);
                _recvMessages.Add(pipe, new ConcurrentQueue<byte[]>());

                return pipe;
            }
        }

        /// <summary>
        /// Удаление созданного или зарегистрированного ранее пайпа по номеру
        /// </summary>
        /// <param name="pipe"></param>
        public void DeletePipe(ushort pipe)
        {
            if (pipe == 0) return;

            lock (this)
            {
                _pipe_recvs.Remove(pipe);
                _recvMessages.Remove(pipe);
            }
        }

        private ushort FindFreeNumber()
        {
            // сервер - четный, клиент - нечетный
            ushort p = 0;
            int n = (_isServer ? 2 : 1);
            while (n <= ushort.MaxValue)
            {
                if (!_pipe_recvs.Keys.Contains((ushort)n))
                {
                    p = (ushort)n;
                    break;
                }
                else
                {
                    n += 2;
                }
            }

            return p;
        }

        public void Close()
        {
            _pipe_recvs.Clear();
            _recvMessages.Clear();
            _waitItems.Clear();

            _isWorking = false;
            _recvAre.Set();
        }

        public Task<byte[]> SendMessageAsync(ushort pipe, byte[] data)
        {
            return Task.Run<byte[]>(() =>
            {
                if (_waitItems.ContainsKey(pipe)) return null;

                var sendBuffer = new byte[data.Length + 2];
                sendBuffer[0] = (byte)(pipe & 0xff);
                sendBuffer[1] = (byte)(pipe >> 8);
                Array.Copy(data, 0, sendBuffer, 2, data.Length);

                var mre = new ManualResetEvent(false);
                var waitItem = new WaitRecvItem() { Mre = mre, Data = null };
                _waitItems.Add(pipe, waitItem);

                _transport.SendMessageAsync(sendBuffer, 0, sendBuffer.Length).Wait();
                if (!mre.WaitOne(TIMEOUT)) return null; // по таймауту

                return waitItem.Data;
            });
        }

        public Task SendResponseAsync(ILpReceiver handler, byte[] data)
        {
            return Task.Run(() =>
            {
                if (!_pipe_recvs.Values.Contains(handler)) return;

                var pipe = _pipe_recvs.FirstOrDefault(h => h.Value == handler).Key;
                var sendBuffer = new byte[data.Length + 2];
                sendBuffer[0] = (byte)(pipe & 0xff);
                sendBuffer[1] = (byte)(pipe >> 8);
                Array.Copy(data, 0, sendBuffer, 2, data.Length);

                _transport.SendMessageAsync(sendBuffer, 0, sendBuffer.Length).Wait();
            });
        }

        /// <summary>
        /// Прием сообщений и складирование их в очереди по пайпам
        /// </summary>
        public async Task DoRecv()
        {
            while (_isWorking)
            {
                int count = await _transport.RecvMessageAsync(_recvBuffer);
                if (count <= 2)
                {
                    Close(); break;
                }

                ushort pipe = (ushort)((_recvBuffer[1] >> 8) + _recvBuffer[0]);
                byte[] data = new byte[count - 2];
                Array.Copy(_recvBuffer, 2, data, 0, count - 2);
                if (_recvMessages.ContainsKey(pipe))
                {
                    _recvMessages[pipe].Enqueue(data);
                    _recvAre.Set();
                }
            }
        }

        private void DoRecv1()
        {
            while (_isWorking)
            {
                _recvAre.WaitOne();
                if (!_isWorking) break;

                var pipes = _recvMessages.Keys.OrderBy(k => k).ToArray();
                foreach (var pipe in pipes)
                {
                    if (_recvMessages[pipe].IsEmpty) continue;

                    byte[] data;
                    while (!_recvMessages[pipe].IsEmpty)
                    {
                        while (!_recvMessages[pipe].TryDequeue(out data)) { };

                        lock (_waitItems)
                        {
                            if (_waitItems.ContainsKey(pipe))
                            {
                                var wi = _waitItems[pipe];
                                _waitItems.Remove(pipe);
                                wi.Data = data;
                                wi.Mre.Set();
                            }
                            else if (_pipe_recvs.ContainsKey(pipe))
                            {
                                _pipe_recvs[pipe].OnRecv(data);
                            }
                        }
                    }
                }
            }
        }
    }

    internal class WaitRecvItem
    {
        public ManualResetEvent Mre { get; set; }
        public byte[] Data { get; set; }
    }
}
