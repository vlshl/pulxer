using Common;
using Common.Data;
using LeechPipe;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class TickPipeServer
    {
        private ILpCore _core;
        private ushort _pipe;
        private readonly ILogger _logger;
        private readonly InstrumCache _instrumCache;

        public TickPipeServer(ILpCore core, ushort pipe, InstrumCache instrumCache, ILogger logger)
        {
            _core = core;
            _pipe = pipe;
            _instrumCache = instrumCache;
            _logger = logger;
        }

        /// <summary>
        /// Получить последние сделки в текущей сессии по указанным инструментам
        /// </summary>
        /// <param name="tickerList">Список тикеров инструментов через запятую</param>
        /// <returns>Список последних сделок</returns>
        public async Task<LastPrice[]> GetLastPrices(string[] tickerList)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetLastPrices " + tickerList.Join(",")));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<LastPrice[]>(data);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, "GetLastPrices error");
                return null;
            }
        }

        /// <summary>
        /// Последние сделки за текущую торговую сессию
        /// </summary>
        /// <param name="insId">ID инструмента</param>
        /// <param name="skip">Сколько сделок пропустить</param>
        /// <returns>Массив сделок для указанного инструмента на текущий момент, если null - ошибка, пустой массив - нет новых сделок</returns>
        public async Task<Tick[]> GetLastTicks(int insId, int skip)
        {
            var instrum = _instrumCache.GetById(insId);
            if (instrum == null) return null;

            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetLastTicks " + instrum.Ticker + " " + skip.ToString()));
            if (res == null) return null;

            if (res.Length == 1 && res[0] != 0) return null;
            if (res.Length == 1 && res[0] == 0) return new Tick[] { };

            AllTradesEncoding enc = new AllTradesEncoding(instrum.Decimals);
            try
            {
                var list = enc.Decode(res, false);
                return list.Select(r => new Tick(0, r.Ts, instrum.InsID, r.Lots, r.Price)).ToArray();
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, "GetLastTicks error");
                return null;
            }
        }

        /// <summary>
        /// Получить дата и время (MSK) последнего тика у Leech.
        /// До открытия утром новой сессии могут выдаваться тики из старой вчерашней сессии.
        /// </summary>
        /// <returns>Дата и время последнего тика или null. Null означает, что в новой сессии тиков еще не было.</returns>
        public async Task<DateTime?> GetLastTickTs()
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetLastTickTs"));
            if (res == null) // null быть не должно, это ошибка
            {
                _logger?.LogError("GetLastTickTs null result");
                return null;
            }

            if (res.Length == 0) // если еще нет тиков, получаем пустую строку
            {
                return null;
            }

            try
            {
                string dateStr = Encoding.UTF8.GetString(res);
                DateTime d;
                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                {
                    _logger?.LogError("GetLastTickTs incorrect result: " + dateStr);
                    return null;
                }
                return d;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "GetLastTickTs error");
                return null;
            }
        }
    }
}
