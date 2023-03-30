using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer
{
    public class TickHistoryBL : ITickHistoryBL
    {
        private ITickHistoryDA _tickHistoryDA = null;
        private IInstrumBL _instrumBL = null;

        public TickHistoryBL(ITickHistoryDA tickHistoryDA, IInstrumBL instrumBL)
        {
            _tickHistoryDA = tickHistoryDA;
            _instrumBL = instrumBL;
        }

        /// <summary>
        /// Получить тиковые исторические данные
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="date">Дата</param>
        /// <returns>Список тиков или пустой список</returns>
        public Task<IEnumerable<Tick>> GetTicksAsync(int insID, DateTime date)
        {
            return Task.Factory.StartNew<IEnumerable<Tick>>(() => 
            {
                var data = _tickHistoryDA.GetData(insID, date);
                if (data == null || data.Length == 0) return new List<Tick>();

                var instrum = _instrumBL.GetInstrumByID(insID);
                if (instrum == null) return new List<Tick>();

                AllTradesEncoding encoding = new AllTradesEncoding(instrum.Decimals);
                var allTradesTicks = encoding.Decode(data);
                if (allTradesTicks == null || !allTradesTicks.Any()) return new List<Tick>();

                if (allTradesTicks.First().Ts.Date == DateTime.MinValue.Date) // дата минимальная, значит это версия AllTrades 1.0 или 1.1
                {
                    // дату берем из аргументов и прибавляем время из тика, т.к. в тике даты нет, там стоит минимальная дата, но время в тике есть
                    return allTradesTicks.Select(t => new Tick(0, date.Date.AddHours(t.Ts.Hour).AddMinutes(t.Ts.Minute).AddSeconds(t.Ts.Second), 
                        insID, t.Lots, t.Price)).ToList();
                }
                else // в версии 1.2 в тиках есть и дата, и время, потому берем Ts из тика
                {
                    return allTradesTicks.Select(t => new Tick(0, t.Ts, insID, t.Lots, t.Price)).ToList();
                }
            });
        }

        /// <summary>
        /// Список инструментов на дату, для которых имеются исторические тиковые данные
        /// </summary>
        /// <param name="date">Дата</param>
        /// <returns>Список инструментов или пустой список</returns>
        public IEnumerable<Instrum> GetInstrumsByDate(DateTime date)
        {
            var insIDs = _tickHistoryDA.GetInstrums(date);
            if (insIDs == null) return new List<Instrum>();

            List<Instrum> instrums = new List<Instrum>();
            foreach (var insID in insIDs)
            {
                Instrum instrum = _instrumBL.GetInstrumByID(insID);
                if (instrum != null) instrums.Add(instrum);
            }

            return instrums;
        }

        /// <summary>
        /// Список дат по инструменту, на которые имеются исторические тиковые данные
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns>Список дат или пустой список</returns>
        public IEnumerable<DateTime> GetDatesByInstrum(int insID)
        {
            var dates = _tickHistoryDA.GetDates(insID);
            if (dates == null) return new List<DateTime>();

            return dates;
        }
    }
}
