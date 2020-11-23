using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using CommonData = Common.Data;
using System.Threading;
using Common.Data;
using Platform;

namespace Pulxer.History
{
    /// <summary>
    /// Загрузчик исторических данных
    /// </summary>
    public class HistoryDownloader
    {
        private readonly IHistoryProvider _provider;
        private readonly IInstrumBL _instrumBL;
        private readonly IInsStoreBL _insStoreBL;

        public HistoryDownloader(IHistoryProvider provider, IInstrumBL instrumBL, IInsStoreBL insStoreBL)
        {
            _provider = provider;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
        }

        /// <summary>
        /// Асинхронная загрузка исторических данных
        /// </summary>
        /// <param name="toDate">Загрузка по эту дату включительно</param>
        /// <param name="isLastDirty">Данные за последний день неполные</param>
        /// <param name="progress">Объект управления фоновой задачей</param>
        /// <param name="cancel">Отмена длительной операции</param>
        /// <returns>Асинхронная задача загрузки</returns>
        public Task DownloadAllAsync(DateTime toDate, bool isLastDirty, BgTaskProgress progress, CancellationToken cancel)
        {
            return Task.Run(() =>
                {
                    try
                    {
                        var insStores = _insStoreBL.GetActiveInsStores();
                        int count = insStores.Count(); int idx = 1;
                        if (progress != null) progress.OnStart(count > 1);
                        Dictionary<CommonData.InsStore, BgTaskProgress> progresses = new Dictionary<CommonData.InsStore, BgTaskProgress>();
                        if (progress != null)
                        {
                            foreach (var ss in insStores)
                            {
                                string name = "";
                                var instrum = _instrumBL.GetInstrumByID(ss.InsID);
                                if (instrum != null) name = instrum.ShortName;
                                var child = progress.AddChildProgress(name);
                                progresses.Add(ss, child);
                            }
                        }

                        foreach (var insStore in insStores)
                        {
                            if (cancel.IsCancellationRequested)
                            {
                                if (progress != null) progress.OnAbort(); break;
                            }

                            var ssCal = _insStoreBL.GetInsStoreCalendar(insStore.InsStoreID);
                            if (ssCal == null || ssCal.Periods == null) continue;

                            DateTime fromDate;
                            if (ssCal.Periods.Count() > 0)
                            {
                                var lastPeriod = ssCal.Periods.Last();
                                fromDate = lastPeriod.IsLastDirty ? lastPeriod.EndDate : lastPeriod.EndDate.AddDays(1);
                            }
                            else
                            {
                                fromDate = _insStoreBL.GetDefaultStartHistoryDate(toDate, insStore.Tf);
                            }

                            var p = progresses.ContainsKey(insStore) ? progresses[insStore] : null;
                            DownloadAsync(insStore, fromDate, toDate, isLastDirty, true, p, cancel).Wait();
                            if (progress != null) progress.OnProgress((double)idx++ / count * 100);
                        }
                        if (progress != null)
                        {
                            if (cancel.IsCancellationRequested) progress.OnAbort(); else progress.OnComplete();
                        }
                    }
                    catch (Exception ex)
                    {
                        if (progress != null) progress.OnFault(ex);
                    }
                });
        }

        /// <summary>
        /// Асинхронная загрузка исторических данных по фин. инструменту
        /// </summary>
        /// <param name="insStore">Поток данных</param>
        /// <param name="date1">Начальная дата</param>
        /// <param name="date2">Конечная дата</param>
        /// <param name="isLastDirty">Последняя дата содердит неполные данные</param>
        /// <param name="isForward">Загрузка блоками от начальной даты к конечной (иначе от конечной к начальной)</param>
        /// <param name="progress">Объект управления фоновой задачей</param>
        /// <param name="cancel">Объект отмены длительной операции</param>
        /// <returns>Асинхронная задача загрузки</returns>
        public async Task DownloadAsync(CommonData.InsStore insStore, DateTime date1, DateTime date2, bool isLastDirty, bool isForward,
            BgTaskProgress progress, CancellationToken cancel)
        {
            try
            {
                var parts = GetDownloadParts(insStore.Tf, date1, date2, isForward);
                int idx = 1; int count = parts.Count();
                if (progress != null) progress.OnStart(count > 1);

                foreach (var part in parts)
                {
                    if (cancel.IsCancellationRequested)
                    {
                        if (progress != null) progress.OnAbort(); 
                        break;
                    }
                    await SyncDataBlock(insStore, part.Date1, part.Date2, isLastDirty, cancel);
                    if (progress != null) progress.OnProgress((double)idx++ / count * 100);
                }
                if (progress != null) 
                {
                    if (cancel.IsCancellationRequested) progress.OnAbort(); else progress.OnComplete();
                }
            }
            catch(Exception ex)
            {
                if (progress != null) progress.OnFault(ex);
            }
        }

        /// <summary>
        /// Разбивка диапазона загрузки на даты
        /// </summary>
        /// <param name="tf">Таймфрейм</param>
        /// <param name="date1">Начальная дата диапазона</param>
        /// <param name="date2">Конечная дата диапазона</param>
        /// <param name="isForward">Список формируется вперед - от меньших дат к большим</param>
        /// <returns></returns>
        private IEnumerable<DownloadPart> GetDownloadParts(Timeframes tf, DateTime date1, DateTime date2, bool isForward)
        {
            date1 = date1.Date; date2 = date2.Date;

            int days = 1000;
            switch (tf)
            {
                case Timeframes.Tick:
                case Timeframes.Min:
                case Timeframes.Min5:
                    days = 1; break;

                case Timeframes.Min10:
                case Timeframes.Min15:
                    days = 30; break;

                case Timeframes.Min20:
                case Timeframes.Min30:
                case Timeframes.Hour:
                    days = 100; break;
            }

            List<DownloadPart> parts = new List<DownloadPart>();

            if (isForward)
            {
                DateTime d1 = date1;
                do
                {
                    DateTime d2 = d1.AddDays(days - 1);
                    if (d2 > date2) d2 = date2;

                    parts.Add(new DownloadPart() { Date1 = d1, Date2 = d2 });
                    d1 = d1.AddDays(days);
                } while (d1 <= date2);
            }
            else
            {
                DateTime d2 = date2;
                do
                {
                    DateTime d1 = d2.AddDays(-days + 1);
                    if (d1 < date1) d1 = date1;

                    parts.Add(new DownloadPart() { Date1 = d1, Date2 = d2 });
                    d2 = d2.AddDays(-days);
                } while (d2 >= date1);
            }

            return parts;
        }

        /// <summary>
        /// Синхронизация данных для списка инструментов.
        /// Для каждого инструмента синхронизируются все потоки, кроме тиков.
        /// Синхронизация выполняется на размер истории по умолчанию, свой для каждого таймфрейма.
        /// </summary>
        /// <param name="insIDs">Инструменты</param>
        /// <param name="endHistoryDate">Последняя дата истории</param>
        /// <param name="isLastDirty">Последняя дата содердит неполные данные</param>
        /// <param name="cancel">Токен отмены</param>
        /// <returns>Асинхронная задача синхронизации</returns>
        public async Task SyncHistory(IEnumerable<int> insIDs, DateTime endHistoryDate, bool isLastDirty, CancellationToken cancel)
        {
            foreach (int insID in insIDs)
            {
                var insStores = _insStoreBL.GetInsStores(insID);
                foreach (var insStore in insStores)
                {
                    if (insStore.Tf == Timeframes.Tick) continue;

                    DateTime startHistoryDate = _insStoreBL.GetDefaultStartHistoryDate(endHistoryDate, insStore.Tf);
                    var parts = GetDownloadParts(insStore.Tf, startHistoryDate, endHistoryDate, false); // backward
                    foreach (var part in parts)
                    {
                        bool hasData = _insStoreBL.HasData(part.Date1, part.Date2, insStore.InsStoreID);
                        if (hasData) continue;

                        await SyncDataBlock(insStore, part.Date1, part.Date2, isLastDirty, cancel);
                    }
                }
            }
        }

        /// <summary>
        /// Синхронизация данных симулятора для списка инструментов.
        /// Для каждого инструмента синхронизируется минимальный поток (кроме тиков).
        /// Синхронизация выполняется на размер форварда по умолчанию, свой для каждого таймфрейма.
        /// </summary>
        /// <param name="insIDs">Инструменты</param>
        /// <param name="startDate">Начало периода данных</param>
        /// <param name="cancel">Токен отмены</param>
        /// <returns>Асинхронная задача синхронизации</returns>
        public async Task SyncForward(IEnumerable<int> insIDs, DateTime startDate, CancellationToken cancel)
        {
            foreach (int insID in insIDs)
            {
                if (cancel.IsCancellationRequested) break;

                var ins = _instrumBL.GetInstrumByID(insID);
                if (ins == null) continue;

                var insStore = _insStoreBL.GetInsStore(insID, Timeframes.Min); // минимальный ТФ
                if (insStore == null || insStore.Tf == Timeframes.Tick) continue;

                var insStores = _insStoreBL.GetInsStores(insID).ToList(); // список потоков всех кроме минимального, их мы будем синхронизировать особо
                var f_ss = insStores.FirstOrDefault(s => s.Tf == Timeframes.Min);
                if (f_ss != null) insStores.Remove(f_ss);

                DateTime endDate = _insStoreBL.GetDefaultEndForwardDate(startDate, insStore.Tf);
                var parts = GetDownloadParts(insStore.Tf, startDate, endDate, true); // forward
                foreach (var part in parts)
                {
                    if (cancel.IsCancellationRequested) break;

                    bool hasData = _insStoreBL.HasData(part.Date1, part.Date2, insStore.InsStoreID);
                    if (hasData) continue;

                    var bars = await SyncDataBlock(insStore, part.Date1, part.Date2, 
                        part.Date2 >= DateTime.Today, cancel); // синхронизируем поток с минимальным ТФ, данные за сегодня грязные
                    foreach (var ss in insStores) // остальные потоки формируем из минимального
                    {
                        if (cancel.IsCancellationRequested) break;

                        var newBarRow = _insStoreBL.ConvertBars(ss.InsID, ss.Tf, bars, cancel);
                        bool isLastDirty = part.Date2 >= DateTime.Today; // данные за сегодня помечаем как грязные, поскольку не уверены что они полные
                        _insStoreBL.InsertData(ss.InsStoreID, ins.Decimals, newBarRow.Bars, part.Date1, part.Date2,
                            isLastDirty, cancel);
                    }
                }
            }
        }

        /// <summary>
        /// Синхронизация блока данных.
        /// Запрашивает с сервера данные и записывает их в базу.
        /// </summary>
        /// <param name="insStore">Поток данных</param>
        /// <param name="date1">Дата начала блока</param>
        /// <param name="date2">Дата окончания блока</param>
        /// <param name="isLastDirty">true - данные за последнюю дату неполные</param>
        /// <param name="cancel">Токен отмены операции</param>
        /// <returns>Задача, выдающая список баров</returns>
        private async Task<IEnumerable<Bar>> SyncDataBlock(InsStore insStore, DateTime date1, DateTime date2, 
            bool isLastDirty, CancellationToken cancel)
        {
            CommonData.Instrum ins = _instrumBL.GetInstrumByID(insStore.InsID);
            var bars = await _provider.GetDataAsync(ins.Ticker, insStore.Tf, date1, date2);
            if (bars == null) return null;

            _insStoreBL.InsertData(insStore.InsStoreID, ins.Decimals, bars, date1, date2, isLastDirty, cancel);

            return bars;
        }
    }

    /// <summary>
    /// Диапазон дат для загрузки
    /// </summary>
    internal struct DownloadPart
    {
        public DateTime Date1;
        public DateTime Date2;
    }
}
