using Common;
using LeechPipe;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
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
    }
}
