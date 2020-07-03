using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeechPipe
{
    public class SystemLp : ILpReceiver
    {
        private const byte IDENT = 0;
        private const byte IDENT_ACK = 1;
        private const byte CMD_CREATE_PIPE = 2;
        private const byte CMD_CREATE_PIPE_ACK = 3;
        private const byte CMD_DELETE_PIPE = 4;
        private const byte CMD_DELETE_PIPE_ACK = 5;

        private ILpFactory _pipeFactory;
        private LpCore _core;
        private ManualResetEvent _mre = null;


        public SystemLp(ILpFactory pipeFactory, LpCore core)
        {
            _pipeFactory = pipeFactory;
            _core = core;
        }

        /// <summary>
        /// Отправляет запрос на создание нового пайпа другой стороне.
        /// Возвращает номер созданного пайпа или ошибку (например, таймаут)
        /// </summary>
        /// <param name="pipeInitData"></param>
        /// <returns></returns>
        public Task<ushort> CreatePipeAsync(byte[] pipeInitData)
        {
            return Task.Run<ushort>(async () =>
            {
                if (_mre != null) return 0; // ждем выполнение пред операции
                if (pipeInitData == null) return 0;

                byte[] fullData = new byte[pipeInitData.Length + 1];
                pipeInitData.CopyTo(fullData, 1);
                fullData[0] = CMD_CREATE_PIPE;
                var resp = await _core.SendMessageAsync(0, fullData);

                if (resp != null && resp.Length == 4 && resp[0] == CMD_CREATE_PIPE_ACK && resp[1] == 0x0)
                {
                    return (ushort)(resp[2] + resp[3] * 256); // успешние создание пайпа
                }

                return 0;
            });
        }

        public void OnRecv(byte[] data)
        {
            if (data == null || data.Length < 1) return;

            byte code = data[0];
            if (code == CMD_CREATE_PIPE)
            {
                var ph = _pipeFactory.CreatePipe(data.Skip(1).ToArray());
                if (ph == null)
                {
                    _core.SendResponseAsync(this, new byte[] { CMD_CREATE_PIPE_ACK, 0xff }); // ошибка - фабрика не смогла создать обработчик
                } 
                else
                {
                    ushort pipe = _core.CreatePipe(ph);
                    if (pipe == 0)
                    {
                        _core.SendResponseAsync(this, new byte[] { CMD_CREATE_PIPE_ACK, 0xfe }); // ошибка - нет свободного номера
                    }
                    else
                    {
                        var pipeBytes = BitConverter.GetBytes(pipe);
                        _core.SendResponseAsync(this, new byte[] { CMD_CREATE_PIPE_ACK, 0x0, pipeBytes[0], pipeBytes[1] }); // успешно
                    }
                }
            }
            else if (code == IDENT)
            {
                var identBytes = Encoding.UTF8.GetBytes(_core.GetIdentity());
                byte[] identData = new byte[identBytes.Length + 1];
                identData[0] = IDENT_ACK;
                Array.Copy(identBytes, 0, identData, 1, identBytes.Length);
                _core.SendResponseAsync(this, identData);
            }
        }

        public async Task<string> GetRemoteIdentity()
        {
            var bytes = await _core.SendMessageAsync(0, new byte[] { IDENT });
            if (bytes == null || bytes.Length <= 1 || bytes[0] != IDENT_ACK) return "";

            return Encoding.UTF8.GetString(bytes, 1, bytes.Length - 1);
        }
    }
}
