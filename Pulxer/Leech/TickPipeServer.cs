using Common;
using Common.Data;
using LeechPipe;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class TickPipeServer
    {
        private ILpCore _core;
        private ushort _pipe;

        public TickPipeServer(ILpCore core, ushort pipe)
        {
            _core = core;
            _pipe = pipe;
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
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Последние сделки за текущую торговую сессию
        /// </summary>
        /// <param name="today">Дата торговой сессии (без времени)</param>
        /// <param name="instrum">Инструмент</param>
        /// <param name="skip">Сколько сделок пропустить</param>
        /// <returns>Массив сделок для указанного инструмента на текущий момент, если null - ошибка, пустой массив - нет новых сделок</returns>
        public async Task<Tick[]> GetLastTicks(DateTime today, Instrum instrum, int skip)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetLastTicks " + instrum.Ticker + " " + skip.ToString()));
            if (res == null) return null;

            if (res.Length == 1 && res[0] != 0) return null;
            if (res.Length == 1 && res[0] == 0) return new Tick[] { };

            AllTradesEncoding enc = new AllTradesEncoding(instrum.Decimals);
            try
            {
                var list = enc.Decode(res, false);
                return list.Select(r => new Tick(0, today.Date.AddSeconds(r.Second), instrum.InsID, r.Lots, r.Price)).ToArray();
            }
            catch
            {
                return null;
            }
        }
    }
}
