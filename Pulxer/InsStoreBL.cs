using Common;
using Common.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using CommonData = Common.Data;
using System.Threading.Tasks;
using System.Threading;
using Storage;
using Common.Data;
using Platform;

namespace Pulxer
{
    /// <summary>
    /// InsStore subsystem interface
    /// </summary>
    public interface IInsStoreBL
    {
        CommonData.InsStore GetInsStore(int insID, Timeframes tf);
        CommonData.InsStore GetInsStore(string ticker, Timeframes tf);
        CommonData.InsStore GetInsStore(int insStoreID);
        CommonData.InsStore CreateInsStore(int insID, Timeframes tf);
        CommonData.InsStore CreateInsStore(string ticker, Timeframes tf);
        void CreateInsStores(IEnumerable<string> tickers, Timeframes tf);
        IEnumerable<CommonData.InsStore> GetInsStores(int insID);
        IEnumerable<CommonData.InsStore> GetInsStores(string ticker);
        IEnumerable<CommonData.InsStore> GetAllInsStores();
        IEnumerable<CommonData.InsStore> GetActiveInsStores();
        Task<int> LoadHistoryAsync(BarRow bars, int insID, DateTime date1, DateTime date2, int? insStoreID = null);
        CommonData.InsStore GetLoadHistoryInsStore(int insID, Timeframes tf);
        DateTime? GetStartHistoryDate(int insStoreID, DateTime endHistoryDate, int days);



        IEnumerable<HistoryProviderInstrum> GetHistoryProviderInstrums(IHistoryProvider provider,
            IEnumerable<int> exceptInsIDs);
        void UpdateInsStore(CommonData.InsStore insStore);
        DateTime[] GetFreeDays(IEnumerable<Bar> bars, DateTime date1, DateTime date2, bool isLastDirty);
        InsStoreCalendar GetInsStoreCalendar(int insStoreID);
        void InsertData(int insStoreID, int decimals, IEnumerable<Bar> bars, DateTime date1, DateTime date2,
            bool isLastDirty, CancellationToken cancel);
        bool HasData(DateTime date1, DateTime date2, int insStoreID);
        bool HasHistory(IEnumerable<int> insIDs, DateTime endHistoryDate);
        bool HasForwardData(IEnumerable<int> insIDs, DateTime startDate);
        DateTime GetDefaultStartHistoryDate(DateTime endHistoryDate, Timeframes tf);
        DateTime GetDefaultEndForwardDate(DateTime startDate, Timeframes tf);
        DateTime? GetTradeDate(IEnumerable<int> insIDs, DateTime startDate);
        BarRow ConvertBars(int insID, Timeframes tf, IEnumerable<Bar> minBars, CancellationToken cancel);
    }

    public class InsStoreBL : IInsStoreBL
    {
        private readonly IInstrumBL _instrumBL = null;
        private readonly IInsStoreDA _insStoreDA = null;

        public InsStoreBL(IInstrumBL instrumBL, IInsStoreDA insStoreDA)
        {
            _instrumBL = instrumBL;
            _insStoreDA = insStoreDA;
        }

        /// <summary>
        /// Получить объект InsStore 
        /// Объект InsStore используется для хранения исторических данных по фин. инструменту.
        /// Исторические данные храняться в указанном таймфрейме.
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="tf">ТФ</param>
        /// <returns>InsStore, null-не найден</returns>
        public CommonData.InsStore GetInsStore(int insID, Timeframes tf)
        {
            return _insStoreDA.GetInsStore(insID, tf);
        }

        /// <summary>
        /// Получить объект InsStore 
        /// Объект InsStore используется для хранения исторических данных по фин. инструменту.
        /// Исторические данные храняться в указанном таймфрейме.
        /// </summary>
        /// <param name="ticker">Инструмент</param>
        /// <param name="tf">ТФ</param>
        /// <returns>InsStore, null - тикер не найден либо InsStore не найден</returns>
        public CommonData.InsStore GetInsStore(string ticker, Timeframes tf)
        {
            var instrum = _instrumBL.GetInstrum(ticker);
            if (instrum == null) return null;

            return _insStoreDA.GetInsStore(instrum.InsID, tf);
        }

        /// <summary>
        /// Получить объект InsStore по ID
        /// </summary>
        /// <param name="insStoreID">Идентификатор</param>
        /// <returns>InsStore or null</returns>
        public CommonData.InsStore GetInsStore(int insStoreID)
        {
            return _insStoreDA.GetInsStoreByID(insStoreID);
        }

        /// <summary>
        /// Создать объект InsStore, если не существует 
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="tf">ТФ</param>
        /// <returns>InsStore</returns>
        public CommonData.InsStore CreateInsStore(int insID, Timeframes tf)
        {
            var insStore = _insStoreDA.GetInsStore(insID, tf);
            if (insStore == null)
            {
                int insStoreID = _insStoreDA.CreateInsStore(insID, tf, true);
                insStore = _insStoreDA.GetInsStoreByID(insStoreID);
            }

            return insStore;
        }

        /// <summary>
        /// Создать объект InsStore, если не существует 
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="tf">ТФ</param>
        /// <returns>InsStore</returns>
        public CommonData.InsStore CreateInsStore(string ticker, Timeframes tf)
        {
            var instrum = _instrumBL.GetInstrum(ticker);
            if (instrum == null) return null;

            return CreateInsStore(instrum.InsID, tf);
        }

        /// <summary>
        /// Создание потоков исторических данных по тикерам
        /// </summary>
        /// <param name="tickers">Список тикеров</param>
        /// <param name="tf">Таймфрейм</param>
        public void CreateInsStores(IEnumerable<string> tickers, Timeframes tf)
        {
            foreach (var ticker in tickers)
            {
                CreateInsStore(ticker, tf);
            }
        }

        /// <summary>
        /// Список потоков данных
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns>Список (может быть пустой)</returns>
        public IEnumerable<CommonData.InsStore> GetInsStores(int insID)
        {
            return _insStoreDA.GetInsStores(insID, null, null);
        }

        /// <summary>
        /// Список потоков данных
        /// </summary>
        /// <param name="ticker">Тикер инструмента</param>
        /// <returns>Список (можкт быть пустой)</returns>
        public IEnumerable<CommonData.InsStore> GetInsStores(string ticker)
        {
            var instrum = _instrumBL.GetInstrum(ticker);
            if (instrum == null) return new List<CommonData.InsStore>();

            return GetInsStores(instrum.InsID);
        }

        /// <summary>
        /// Список потоков данных
        /// </summary>
        /// <returns>Список (можкт быть пустой)</returns>
        public IEnumerable<CommonData.InsStore> GetAllInsStores()
        {
            return _insStoreDA.GetInsStores(null, null, null);
        }

        /// <summary>
        /// Список активных потоков данных
        /// </summary>
        /// <returns>Список (можкт быть пустой)</returns>
        public IEnumerable<CommonData.InsStore> GetActiveInsStores()
        {
            return _insStoreDA.GetInsStores(null, null, true);
        }

        /// <summary>
        /// Загрузка исторических данных в BarRow.
        /// Наиболее подходящий InsStore определяется автоматически.
        /// </summary>
        /// <param name="bars">BarRow</param>
        /// <param name="insID">Инструмент</param>
        /// <param name="date1">Нач дата</param>
        /// <param name="date2">Кон дата</param>
        /// <param name="insStoreID">Поток данных для загрузки (если null, то поток будет определен автоматически)</param>
        /// <returns>Асинхронная задача загрузки. Общее число баров после загрузки.</returns>
        public async Task<int> LoadHistoryAsync(BarRow bars, int insID, DateTime date1, DateTime date2, int? insStoreID = null)
        {
            Instrum instrum = _instrumBL.GetInstrumByID(insID);
            if (instrum == null) return 0;

            if (insStoreID == null)
            {
                var insStore = GetLoadHistoryInsStore(insID, bars.Timeframe);
                if (insStore != null) insStoreID = insStore.InsStoreID;
            }
            if (insStoreID == null) return 0;

            int k = (int)Math.Pow(10, instrum.Decimals);
            var list = await _insStoreDA.GetHistoryAsync(insStoreID.Value, date1, date2);

            return await Task.Run(() =>
            {
                bars.SuspendEvents();
                foreach (var bar in list)
                {
                    DateTime time = StorageLib.ToDateTime(bar.Time);
                    decimal openPrice = (decimal)bar.OpenPrice / k;
                    decimal lowPrice = (decimal)(bar.OpenPrice + bar.LowDelta) / k;
                    decimal highPrice = (decimal)(bar.OpenPrice + bar.HighDelta) / k;
                    decimal closePrice = (decimal)(bar.OpenPrice + bar.CloseDelta) / k;

                    bars.AddTick(time, openPrice, 0);
                    bars.AddTick(time, lowPrice, 0);
                    bars.AddTick(time, highPrice, 0);
                    bars.AddTick(time, closePrice, bar.Volume);
                }
                bars.CloseLastBar();
                bars.ResumeEvents();

                return bars.Count;
            });
        }

        /// <summary>
        /// Наилучший insStore для загрузки.
        /// Например, требуется сформировать бары с таймфреймом Hour, но такого insStore в базе нет. 
        /// Тогда из имеющихся insStore будет выбран наиболее подходящий.
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="tf">ТФ у BarRow</param>
        /// <returns>Наиболее подходящий insStore для загрузки.</returns>
        public CommonData.InsStore GetLoadHistoryInsStore(int insID, Timeframes tf)
        {
            CommonData.InsStore ss = null;
            var insStores = GetInsStores(insID);

            if (tf == Timeframes.Tick)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min5)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min10)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min15)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min20)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Min30)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Hour)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Hour);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Day)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Day);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Hour);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            if (tf == Timeframes.Week)
            {
                ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Week);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Day);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Hour);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min30);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min20);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min15);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min10);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min5);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (ss == null)
                    ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Tick);
            }

            return ss;
        }

        /// <summary>
        /// Поиск начала исторических данных.
        /// Начальная дата ищется с таким расчетом, чтобы в период исторческих данных попало нужное число торговых дней.
        /// То есть выходные дни (когда не было торгов) пропускаются и начальная дата смещается влево.
        /// </summary>
        /// <param name="insStoreID">Идентификатор потока данных</param>
        /// <param name="endHistoryDate">Конечная дата исторических данных</param>
        /// <param name="days">Требуемое число торговых дней</param>
        /// <returns>Дата начала исторических данных. Если null - начальную дату определить не удалось, например, если в базе недостаточно данных.</returns>
        public DateTime? GetStartHistoryDate(int insStoreID, DateTime endHistoryDate, int days)
        {
            if (days <= 0) return null;

            var calendar = GetInsStoreCalendar(insStoreID);
            var period = calendar.Periods.FirstOrDefault(p => p.StartDate <= endHistoryDate && endHistoryDate <= p.EndDate);
            if (period == null) return null;

            if (period.EndDate == endHistoryDate && period.IsLastDirty) return null;

            DateTime date = endHistoryDate;

            while (date >= period.StartDate)
            {
                if (!calendar.FreeDays.Contains(date))
                {
                    days--;
                }
                if (days == 0) break;

                date = date.AddDays(-1);
            }
            if (days > 0) return null;

            return date;
        }










        /// <summary>
        /// Список инструментов, предоставляемых провайдером исторических данных,
        /// исключая указанные insID
        /// </summary>
        /// <param name="provider">Провайден исторических данных</param>
        /// <param name="exceptInsIDs">Список ID инструментов, которые следует исключить из итогового списка. Если null или пустой, то никакие инструменты не исключаются.</param>
        /// <returns></returns>
        public IEnumerable<HistoryProviderInstrum> GetHistoryProviderInstrums(IHistoryProvider provider,
            IEnumerable<int> exceptInsIDs)
        {
            if (provider == null) throw new ArgumentException("provider");

            List<CommonData.Instrum> exceptInstrums = new List<Common.Data.Instrum>();
            if (exceptInsIDs != null)
            {
                foreach (int insID in exceptInsIDs)
                {
                    var ins = _instrumBL.GetInstrumByID(insID);
                    if (ins == null || ins.InsID == 0) continue;
                    exceptInstrums.Add(ins);
                }
            }

            var list = provider.GetInstrums().Where(hps =>
                exceptInstrums.FirstOrDefault(s => s.Ticker == hps.Ticker) == null).ToList(); // пока сравнение идет только по тикеру

            return list;
        }

        /// <summary>
        /// Обновить объект InsStore
        /// </summary>
        /// <param name="insStore"></param>
        public void UpdateInsStore(CommonData.InsStore insStore)
        {
            _insStoreDA.UpdateInsStore(insStore.InsStoreID, insStore.IsEnable);
        }


        /// <summary>
        /// Получить список дат из промежутка от date1 по date2, для которых нет данных,
        /// т.е. список дней когда торгов не было
        /// </summary>
        /// <param name="bars">Список баров</param>
        /// <param name="date1">Начальная дата</param>
        /// <param name="date2">Конечная дата</param>
        /// <param name="isLastDirty">Если последний день помечен как неполный, то он не может быть свободным</param>
        /// <returns>Список дат, когда не было сделок. Если в date2 сделок не было, но стоит флаг isLastDirty, то date2 не включается в список.</returns>
        public DateTime[] GetFreeDays(IEnumerable<Bar> bars, DateTime date1, DateTime date2,
            bool isLastDirty)
        {
            date1 = date1.Date; date2 = date2.Date;
            if (date1 > date2)
                throw new ArgumentException("date1 > date2");

            var tradeDays = (from b in bars select b.Time.Date).Distinct().ToList();
            List<DateTime> freeDates = new List<DateTime>();
            DateTime endDate = isLastDirty ? date2.AddDays(-1) : date2;
            for (DateTime d = date1; d <= endDate; d = d.AddDays(1))
            {
                if (tradeDays.Contains(d)) continue;
                freeDates.Add(d);
            }

            return freeDates.ToArray();
        }

        /// <summary>
        /// Запись в базу исторических данных
        /// </summary>
        /// <param name="insStoreID">Поток данных</param>
        /// <param name="decimals">Кол-во десятичных знаков после запятой в ценах</param>
        /// <param name="bars">Список баров</param>
        /// <param name="date1">Начальная дата блока исторических данных</param>
        /// <param name="date2">Конечная дата блока исторических данных</param>
        /// <param name="isLastDirty">Конечная дата помечается как неполный день</param>
        /// <param name="cancel">Токен отмены длительной операции</param>
        public void InsertData(int insStoreID, int decimals, IEnumerable<Bar> bars, DateTime date1, DateTime date2,
            bool isLastDirty, CancellationToken cancel)
        {
            var calendar = GetInsStoreCalendar(insStoreID);
            if (calendar == null) return;

            calendar.AppendPeriod(new InsStorePeriod(date1, date2, isLastDirty));
            var freeDays = GetFreeDays(bars, date1, date2, isLastDirty);
            calendar.UpdateFreeDays(freeDays, date1, date2);

            int k = (int)Math.Pow(10, decimals);

            var dbBars = bars.Select(b => 
            {
                DbBarHistory bh = new DbBarHistory();
                bh.InsStoreID = insStoreID;
                bh.Time = StorageLib.ToDbTime(b.Time);

                int op = (int)(b.Open * k);
                int cp = (int)(b.Close * k);
                int hp = (int)(b.High * k);
                int lp = (int)(b.Low * k);

                bh.OpenPrice = op;
                bh.CloseDelta = CalcDelta(cp, op);
                bh.HighDelta = CalcDelta(hp, op);
                bh.LowDelta = CalcDelta(lp, op);

                long v = b.Volume;
                if (v > int.MaxValue) v = int.MaxValue;
                if (v < int.MinValue) v = int.MinValue;
                bh.Volume = (int)v;

                return bh;
            });

            _insStoreDA.InsertBars(insStoreID, dbBars, date1, date2, cancel);
            if (cancel.IsCancellationRequested) return;

            _insStoreDA.UpdatePeriods(insStoreID, calendar.Periods);
            _insStoreDA.UpdateFreeDays(insStoreID, calendar.FreeDays);
        }

        private short CalcDelta(int p1, int p)
        {
            int d = p1 - p;
            if (d > short.MaxValue) d = short.MaxValue;
            if (d < short.MinValue) d = short.MinValue;

            return (short)d;
        }

        /// <summary>
        /// Получить объект календаря по потоку данных
        /// </summary>
        /// <param name="insStoreID">Поток данных</param>
        /// <returns>Объект календаря</returns>
        public InsStoreCalendar GetInsStoreCalendar(int insStoreID)
        {
            if (insStoreID_calendar.ContainsKey(insStoreID))
            {
                return insStoreID_calendar[insStoreID];
            }
            else
            {
                var calendar = new InsStoreCalendar();
                var periods = _insStoreDA.GetPeriods(insStoreID);
                var freeDays = _insStoreDA.GetFreeDays(insStoreID);
                if (periods != null)
                {
                    foreach (var p in periods) calendar.AppendPeriod(p);
                }
                if (freeDays != null) calendar.AddFreeDays(freeDays);

                insStoreID_calendar.Add(insStoreID, calendar);

                return calendar;
            }
        }
        private Dictionary<int, InsStoreCalendar> insStoreID_calendar = new Dictionary<int, InsStoreCalendar>();

        /// <summary>
        /// Наличие данных за указанный период
        /// </summary>
        /// <param name="date1">Начальная дата</param>
        /// <param name="date2">Конечная дата</param>
        /// <param name="insStoreID">Поток данных</param>
        /// <returns>true - данные за указанный период загружены (хотя в некоторые дни сделок может не быть, это значит в этот день не было торгов)</returns>
        public bool HasData(DateTime date1, DateTime date2, int insStoreID)
        {
            var calendar = GetInsStoreCalendar(insStoreID);
            if (calendar == null) return false;

            var foundPeriod = calendar.Periods.FirstOrDefault(p => p.StartDate <= date1 && date2 <= p.EndDate);

            return foundPeriod != null;
        }

        /// <summary>
        /// Имеется ли достаточное количество историческмх данных для всех инструментов
        /// </summary>
        /// <param name="insIDs">Список инструментов</param>
        /// <param name="endHistoryDate">Последний день исторических данных</param>
        /// <returns>true - исторических данных достаточно</returns>
        public bool HasHistory(IEnumerable<int> insIDs, DateTime endHistoryDate)
        {
            foreach (int insID in insIDs)
            {
                var insStores = GetInsStores(insID);
                foreach (var insStore in insStores)
                {
                    if (insStore.Tf == Timeframes.Tick) continue;

                    DateTime startHistoryDate = GetDefaultStartHistoryDate(endHistoryDate, insStore.Tf);
                    bool hasData = HasData(startHistoryDate, endHistoryDate, insStore.InsStoreID);
                    if (!hasData) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Имеется ли достаточное количество данных с минимальным таймфреймом для работы симулятора
        /// </summary>
        /// <param name="insIDs">Список инструментов</param>
        /// <param name="startDate">Начальная дата периода тестирования</param>
        /// <returns>true - данных для работы симулятора достаточно</returns>
        public bool HasForwardData(IEnumerable<int> insIDs, DateTime startDate)
        {
            foreach (int insID in insIDs)
            {
                var ins = _instrumBL.GetInstrumByID(insID);
                if (ins == null) return false;
                var insStore = _insStoreDA.GetInsStore(insID, Timeframes.Min); // поток по умолчанию (минимальный)
                if (insStore == null) return false;

                DateTime endDate = GetDefaultEndForwardDate(startDate, insStore.Tf);
                bool hasData = HasData(startDate, endDate, insStore.InsStoreID);
                if (!hasData) return false;
            }

            return true;
        }

        /// <summary>
        /// Найти ближайший торговый день (поиск начинается с startDate, т.е. startDate тоже может стать результатом поиска)
        /// Указанная дата должна изначально попадать в синхронизированные периоды
        /// </summary>
        /// <param name="insIDs">Список инструментов</param>
        /// <param name="startDate">Поиск начинается с этой даты</param>
        /// <returns>Null-не удалось найти</returns>
        public DateTime? GetTradeDate(IEnumerable<int> insIDs, DateTime startDate)
        {
            if (insIDs == null || insIDs.Count() == 0)
                return null;

            List<InsStoreCalendar> calendars = new List<InsStoreCalendar>();
            foreach (int insID in insIDs)
            {
                var ins = _instrumBL.GetInstrumByID(insID);
                if (ins == null) continue;
                var insStore = _insStoreDA.GetInsStore(insID, Timeframes.Min); // поток по умолчанию (минимальный)
                if (insStore == null) continue;

                var calendar = GetInsStoreCalendar(insStore.InsStoreID);
                if (calendar == null) continue;

                calendars.Add(calendar);
            }

            if (calendars.Count == 0 || calendars.Count != insIDs.Count()) // проверка на ноль лишняя, но на всякий случай пусть будет
                return null;

            DateTime date = startDate.Date;
            while (true)
            {
                // если хоть в одном календаре не нашлось периода с этой датой, поиски прекращаем
                // последний грязный день в периоде не считаем
                if (calendars.Any(c => c.Periods.FirstOrDefault(p =>
                    p.StartDate <= date && date <= (p.IsLastDirty ? p.EndDate.AddDays(-1) : p.EndDate)) == null))
                    return null;
                // во всех календарях есть период, куда попадает эта дата
                // если во всех календарях дата не является праздником, то это та дата, которую мы ищем
                if (calendars.All(c => !c.FreeDays.Contains(date)))
                    return date;

                // дата есть во всех календарях, но она праздничная, значит попробуем следующую
                date = date.AddDays(1);
            }
        }

        /// <summary>
        /// Оптимальная начальная дата исторических данных
        /// </summary>
        /// <param name="endHistoryDate">Последняя дата исторических данных</param>
        /// <param name="tf">Таймфрейм</param>
        /// <returns>В зависимости от таймфрейма вычисляется оптимальный размер исторических данных и вычисляется их начальная дата</returns>
        public DateTime GetDefaultStartHistoryDate(DateTime endHistoryDate, Timeframes tf)
        {
            DateTime hist = endHistoryDate;
            switch (tf)
            {
                case Timeframes.Tick: hist = endHistoryDate.AddDays(-10); break;
                case Timeframes.Min: hist = endHistoryDate.AddMonths(-1); break;
                case Timeframes.Min5: hist = endHistoryDate.AddMonths(-2); break;
                case Timeframes.Min10: hist = endHistoryDate.AddMonths(-3); break;
                case Timeframes.Min15: hist = endHistoryDate.AddMonths(-4); break;
                case Timeframes.Min20: hist = endHistoryDate.AddMonths(-5); break;
                case Timeframes.Min30: hist = endHistoryDate.AddMonths(-6); break;
                case Timeframes.Hour: hist = endHistoryDate.AddYears(-1); break;
                case Timeframes.Day: hist = endHistoryDate.AddYears(-5); break;
                case Timeframes.Week: hist = endHistoryDate.AddYears(-5); break;
            }

            return hist;
        }

        /// <summary>
        /// Оптимальная конечная дата для работы симулятора
        /// </summary>
        /// <param name="startDate">Начальная дата работы симулятора</param>
        /// <param name="tf">Таймфрейм</param>
        /// <returns>В зависимости от таймфрейма вычисляется оптимальная конечная дата работы симулятора. Перед запуском симулятора данные по конечную дату должны быть загружены с сервера.</returns>
        public DateTime GetDefaultEndForwardDate(DateTime startDate, Timeframes tf)
        {
            DateTime hist = startDate;
            switch (tf)
            {
                case Timeframes.Tick: hist = startDate.AddDays(10); break;
                case Timeframes.Min: hist = startDate.AddMonths(1); break;
                case Timeframes.Min5: hist = startDate.AddMonths(2); break;
                case Timeframes.Min10: hist = startDate.AddMonths(3); break;
                case Timeframes.Min15: hist = startDate.AddMonths(4); break;
                case Timeframes.Min20: hist = startDate.AddMonths(5); break;
                case Timeframes.Min30: hist = startDate.AddMonths(6); break;
                case Timeframes.Hour: hist = startDate.AddYears(1); break;
                case Timeframes.Day: hist = startDate.AddYears(5); break;
                case Timeframes.Week: hist = startDate.AddYears(5); break;
            }

            return hist;
        }

        /// <summary>
        /// Получить массив баров по другому массиву баров (более мелкому)
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <param name="tf">ТФ нового массива</param>
        /// <param name="minBars">Старый массив</param>
        /// <param name="cancel">Токен отмены</param>
        /// <returns>Новый массив баров из старого</returns>
        public BarRow ConvertBars(int insID, Timeframes tf, IEnumerable<Bar> minBars, CancellationToken cancel)
        {
            BarRow barRow = new BarRow(tf, insID);
            foreach (var minBar in minBars)
            {
                if (cancel.IsCancellationRequested) break;

                barRow.AddTick(minBar.Time, minBar.Open, 0);
                barRow.AddTick(minBar.Time, minBar.High, 0);
                barRow.AddTick(minBar.Time, minBar.Low, 0);
                barRow.AddTick(minBar.Time, minBar.Close, minBar.Volume);
            }
            barRow.CloseLastBar();

            return barRow;
        }
    }
}
