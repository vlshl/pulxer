using LeechPipe;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class TickHistoryPipeServer
    {
        private ILpCore _core;
        private ushort _pipe;
        private readonly ILogger _logger;

        public TickHistoryPipeServer(ILpCore core, ushort pipe, ILogger logger)
        {
            _core = core;
            _pipe = pipe;
            _logger = logger;
        }

        /// <summary>
        /// Получить список дат за указанный год, для которых есть тиковые исторические данные
        /// </summary>
        /// <param name="year">Год</param>
        /// <returns>Список дат (массив строк в формате yyyy-MM-dd) или пустой список, если null - ошибка</returns>
        public async Task<string[]> GetDates(int year)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetDates " + year.ToString()));
            if (res == null)
            {
                _logger?.LogError("GetDates: null result");
                return null;
            }
            if ((res.Length == 1) && (res[0] != 0))
            {
                _logger?.LogError("GetDates error:" + res[0].ToString());
                return null;
            }

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<string[]>(data);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetDates error");
                return null;
            }
        }

        /// <summary>
        /// Получить список тикеров на указанную дату, для которых есть исторические тиковые данные
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Список тикеров или пустой список, если null - ошибка</returns>
        public async Task<string[]> GetTickers(DateTime date)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetTickers " + date.ToString("yyyy-MM-dd")));
            if (res == null)
            {
                _logger?.LogError("GetTickers: null result");
                return null;
            }
            if ((res.Length == 1) && (res[0] != 0))
            {
                _logger?.LogError("GetTickers error:" + res[0].ToString());
                return null;
            }

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<string[]>(data);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetTickers error");
                return null;
            }
        }

        /// <summary>
        /// Получить данные по сделкам в формате AllTrades
        /// </summary>
        /// <param name="date">Дата</param>
        /// <param name="ticker">Тикер</param>
        /// <returns>Массив байт в формате AllTrades, если пустой массив - нет данных, если null - ошибка</returns>
        public async Task<byte[]> GetData(DateTime date, string ticker)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetData " + date.ToString("yyyy-MM-dd") + " " + ticker));
            if (res == null)
            {
                _logger?.LogError("GetData: null result");
                return null;
            }
            if ((res.Length == 1) && (res[0] != 0))
            {
                _logger?.LogError("GetData error:" + res[0].ToString());
                return null;
            }

            return res;
        }

        public ushort GetPipe()
        {
            return _pipe;
        }
    }
}
