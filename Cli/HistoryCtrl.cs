using Common;
using Common.Interfaces;
using Platform;
using Pulxer;
using Pulxer.History;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using CommonData = Common.Data;

namespace Cli
{
    public class HistoryCtrl : BgCtrl
    {
        private readonly IConsole _console;
        private readonly IInsStoreBL _insStoreBL;
        private readonly IInstrumBL _instrumBL;
        private readonly ITickHistoryBL _tickHistoryBL;
        private readonly HistoryDownloader _historyDownloader;

        public HistoryCtrl(IConsole console, IInsStoreBL insStoreBL, IInstrumBL instrumBL, ITickHistoryBL tickHistoryBL,
            HistoryDownloader historyDownloader) : base(console)
        {
            _console = console;
            _insStoreBL = insStoreBL;
            _instrumBL = instrumBL;
            _tickHistoryBL = tickHistoryBL;
            _historyDownloader = historyDownloader;
        }

        public void HistoryDownloadAll(List<string> args)
        {
            bool isLastDirty = true;
            DateTime toDate = DateTime.Today;
            bool isProgress = false;
            bool isCancel = false;
            DateTime d;

            foreach (var arg in args)
            {
                if (DateTime.TryParse(arg, out d))
                {
                    toDate = d;
                    continue;
                }
                if (arg.ToLower() == "full")
                {
                    isLastDirty = false;
                }
                if (arg.ToLower() == "progress")
                {
                    isProgress = true;
                }
                if (arg.ToLower() == "cancel")
                {
                    isCancel = true;
                }
            }

            if (isProgress)
            {
                if (_progress != null)
                {
                    ShowProgress(_progress);
                }
                else
                {
                    _console.WriteLine("Нет фоновой операции");
                }
                return;
            }

            if (isCancel)
            {
                if (_cancel != null)
                {
                    _cancel.Cancel();
                    _console.WriteLine("Прервано");
                }
                return;
            }

            if (_progress == null || !_progress.IsWorking)
            {
                _console.WriteLine("Загрузка котировок: toDate = " + toDate.ToString("dd/MM/yyyy") + ", isLastDirty = " + isLastDirty.ToString());
                _progress = new BgTaskProgress(_syncContext, "Загрузка данных ...");
                _cancel = new CancellationTokenSource();
                _historyDownloader.DownloadAllAsync(toDate, isLastDirty, _progress, _cancel.Token);
            }
            else
            {
                ShowProgress(_progress);
            }
        }

        /// <summary>
        /// Загрузка одного потока
        /// </summary>
        /// <param name="args">Date1, Date2, [Dirty], Tf, Ticker, Ticker, ...</param>
        public async void HistoryDownloadAsync(List<string> args)
        {
            DateTime date1 = DateTime.Today;
            DateTime date2 = DateTime.Today;
            bool isLastDirty = false;
            Timeframes tf = Timeframes.Min;

            if (args.Count == 1)
            {
                if (args[0].Trim().ToLower() == "progress")
                {
                    if (_progress != null)
                    {
                        ShowProgress(_progress);
                    }
                    else
                    {
                        _console.WriteLine("Нет операции");
                    }
                    return;
                }
                else if (args[0].Trim().ToLower() == "cancel")
                {
                    if (_cancel != null)
                    {
                        _cancel.Cancel();
                        _console.WriteLine("Операция прервана");
                    }
                    else
                    {
                        _console.WriteLine("Нет операции");
                    }
                    return;
                }
            }

            if (args.Count < 2)
            {
                _console.WriteError("Не указаны даты");
                return;
            }

            DateTime d;
            if (DateTime.TryParse(args[0].Trim(), out d))
            {
                date1 = d;
            }
            else
            {
                _console.WriteError("Неверно указана дата начала");
                return;
            }
            if (DateTime.TryParse(args[1].Trim(), out d))
            {
                date2 = d;
            }
            else
            {
                _console.WriteError("Неверно указана дата окончания");
                return;
            }

            args.RemoveRange(0, 2);

            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            if (args[0].Trim().ToLower() == "dirty")
            {
                isLastDirty = true;
                args.RemoveAt(0);
            }

            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            Timeframes tf_;
            if (!Timeframes.TryParse<Timeframes>(args[0].Trim(), out tf_))
            {
                _console.WriteError("Неверный агрумент: Timeframe");
                return;
            }
            tf = tf_;

            args.RemoveAt(0);

            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            List<CommonData.InsStore> insStores = new List<CommonData.InsStore>();
            foreach (string ticker in args)
            {
                CommonData.InsStore insStore = _insStoreBL.GetInsStore(ticker, tf);
                if (insStore == null)
                {
                    _console.WriteError("Не найден тикер: " + ticker);
                    continue;
                }
                insStores.Add(insStore);
            }
            if (insStores.Count == 0)
            {
                _console.WriteError("Пустой список тикеров");
                return;
            }

            if (_progress != null && _progress.IsWorking)
            {
                ShowProgress(_progress);
                return;
            }

            _console.WriteLine("Загрузка данных ... ");
            _progress = new BgTaskProgress(_syncContext, "Загрузка данных ...");
            _cancel = new CancellationTokenSource();

            _progress.OnStart();
            int idx = 0;
            foreach (var insStore in insStores)
            {
                var instrum = _instrumBL.GetInstrumByID(insStore.InsID);
                if (instrum == null) continue;

                var p = _progress.AddChildProgress(instrum.ShortName);
                await _historyDownloader.DownloadAsync(insStore, date1, date2, isLastDirty, true, p, _cancel.Token);
                idx++;
                _progress.OnProgress((double)idx * 100 / insStores.Count);
            }
            _progress.OnComplete();
        }

        /// <summary>
        /// Вывод исторических данных
        /// </summary>
        /// <param name="args">Tf, Ticker, Date1, Date2</param>
        public void GetBars(List<string> args)
        {
            if (args.Count < 4)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            Timeframes tf;
            if (!Timeframes.TryParse<Timeframes>(args[0], out tf))
            {
                _console.WriteError("Неверный агрумент: Timeframe");
                return;
            }

            string ticker = args[1];
            var instrum = _instrumBL.GetInstrum(ticker);
            if (instrum == null)
            {
                _console.WriteError("Тикер не найден");
                return;
            }

            DateTime date1, date2;
            DateTime d;
            if (DateTime.TryParse(args[2].Trim(), out d))
            {
                date1 = d;
            }
            else
            {
                _console.WriteError("Неверно указана дата начала");
                return;
            }
            if (DateTime.TryParse(args[3].Trim(), out d))
            {
                date2 = d;
            }
            else
            {
                _console.WriteError("Неверно указана дата окончания");
                return;
            }

            BarRow bars = new BarRow(tf, instrum.InsID);
            _insStoreBL.LoadHistoryAsync(bars, instrum.InsID, date1, date2).Wait();

            _console.WriteLine("Time\t\t\tOpen\tHigh\tLow\tClose\tVolume");
            _console.WriteSeparator();
            string format = "0." + (new string('0', instrum.Decimals));
            foreach (var bar in bars.Bars)
            {
                _console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                    bar.Time.ToString("dd.MM.yyyy HH:mm:ss"),
                    bar.Open.ToString(format),
                    bar.High.ToString(format),
                    bar.Low.ToString(format),
                    bar.Close.ToString(format),
                    bar.Volume));
            }
            _console.WriteSeparator();
            _console.WriteLine("Count: " + bars.Count.ToString());
        }

        /// <summary>
        /// Список дат по тикеру для тиковых исторических данных
        /// </summary>
        /// <param name="args">Ticker</param>
        public void GetTickHistoryDates(List<string> args)
        {
            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            string ticker = args[0];
            var instrum = _instrumBL.GetInstrum(ticker);
            if (instrum == null)
            {
                _console.WriteError("Тикер не найден");
                return;
            }

            int year = 0;
            if (args.Count >= 2)
            {
                if (!int.TryParse(args[1], out year))
                {
                    year = 0;
                    _console.WriteError("Год указан неверно, выводится информация за все годы.");
                }
            }

            var dates = _tickHistoryBL.GetDatesByInstrum(instrum.InsID);
            if (year > 0)
            {
                dates = dates.Where(d => d.Year == year);
            }

            if (dates.Any())
            {
                Calendar calendar = CultureInfo.InvariantCulture.Calendar;
                dates = dates.OrderBy(d => d).ToList();
                int w = -1;
                int y = 0;

                string[] days = { "          ", "          ", "          ", "          ", "          ", "          ", "          " };

                foreach (var date in dates)
                {
                    int w1 = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                    if (y != date.Year || w != w1)
                    {
                        if (days.Any(d => d.Trim().Length > 0))
                        {
                            _console.WriteLine(string.Format("{0}   {1}   {2}   {3}   {4}   {5}   {6}", days[0], days[1], days[2], days[3], days[4], days[5], days[6]));
                            days[0] = days[1] = days[2] = days[3] = days[4] = days[5] = days[6] = "          ";
                        }
                    }

                    if (y != date.Year)
                    {
                        _console.WriteLine("----Пн----   ----Вт----   ----Ср----   ----Чт----   ----Пт----   ---Сб-----   ----Вс----   [ " + date.Year + " ]");
                    }

                    w = w1;
                    y = date.Year;

                    string ds = date.ToString("dd.MM.yyyy");
                    switch (date.DayOfWeek)
                    {
                        case DayOfWeek.Monday: days[0] = ds; break;
                        case DayOfWeek.Tuesday: days[1] = ds; break;
                        case DayOfWeek.Wednesday: days[2] = ds; break;
                        case DayOfWeek.Thursday: days[3] = ds; break;
                        case DayOfWeek.Friday: days[4] = ds; break;
                        case DayOfWeek.Saturday: days[5] = ds; break;
                        case DayOfWeek.Sunday: days[6] = ds; break;
                    }
                }
                if (days.Any(d => d.Trim().Length > 0))
                {
                    _console.WriteLine(string.Format("{0}   {1}   {2}   {3}   {4}   {5}   {6}", days[0], days[1], days[2], days[3], days[4], days[5], days[6]));
                }
            }
            else
            {
                _console.WriteLine("Нет данных");
            }
        }


        /// <summary>
        /// Список инструментов по дате для тиковых исторических данных
        /// </summary>
        /// <param name="args">Дата</param>
        public void GetTickHistoryInstrums(List<string> args)
        {
            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            DateTime date;
            DateTime d;
            if (DateTime.TryParse(args[0].Trim(), out d))
            {
                date = d;
            }
            else
            {
                _console.WriteError("Неверно указана дата");
                return;
            }

            var instrums = _tickHistoryBL.GetInstrumsByDate(date);
            if (instrums.Any())
            {
                _console.WriteLine("Тикер\tКраткое наименование");
                _console.WriteSeparator();
                foreach (var instrum in instrums)
                {
                    _console.WriteLine(instrum.Ticker + "\t" + instrum.ShortName);
                }
            }
            else
            {
                _console.WriteLine("Нет данных");
            }
        }
    }
}
