using Common;
using LeechPipe;
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
        /// <param name="insIDs">Список идентификаторов инструментов через запятую</param>
        /// <returns>Список последних сделок</returns>
        public async Task<Tick[]> GetLastTicks(string insIDs)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetLastTicks " + insIDs));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                var ticks = JsonConvert.DeserializeObject<Tick[]>(data);
                return ticks;
            }
            catch
            {
                return null;
            }
        }
    }
}
