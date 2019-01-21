using Common.Interfaces;
using Platform;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cli
{
    public class InsStoreCtrl
    {
        private readonly IConsole _console;
        private readonly IInstrumBL _instrumBL;
        private readonly IInsStoreBL _insStoreBL;

        public InsStoreCtrl(IConsole console, IInstrumBL instrumBL, IInsStoreBL insStoreBL)
        {
            _console = console;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
        }

        public void CreateInsStore(List<string> args)
        {
            if (args.Count < 2)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            Timeframes tf;
            if (!Timeframes.TryParse<Timeframes>(args[0], out tf))
            {
                _console.WriteError("Неверный первый агрумент: Timeframe");
                return;
            }

            args.RemoveAt(0);

            _insStoreBL.CreateInsStores(args, tf);

            _console.WriteLine("InsStoreID\tInsID\tTicker\tTf\tIsEnable");
            _console.WriteSeparator();
            foreach (var ticker in args)
            {
                var insStore = _insStoreBL.GetInsStore(ticker, tf);
                if (insStore == null) continue;

                _console.WriteLine(string.Format("{0}\t\t{1}\t{2}\t{3}\t{4}",
                    insStore.InsStoreID.ToString(),
                    insStore.InsID.ToString(),
                    ticker,
                    insStore.Tf.ToString(),
                    insStore.IsEnable ? "*" : "-"));
            }
        }

        public void ListInsStore()
        {
            var list = _insStoreBL.GetAllInsStores();

            _console.WriteLine("InsStoreID\tInsID\tTicker\tTf\tIsEnable");
            _console.WriteSeparator();
            foreach (var insStore in list)
            {
                var instrum = _instrumBL.GetInstrumByID(insStore.InsID);
                if (instrum == null) continue;

                _console.WriteLine(string.Format("{0}\t\t{1}\t{2}\t{3}\t{4}",
                    insStore.InsStoreID.ToString(),
                    insStore.InsID.ToString(),
                    instrum.Ticker,
                    insStore.Tf.ToString(),
                    insStore.IsEnable ? "*" : "-"));
            }
        }

        /// <summary>
        /// Информация по потоку данных
        /// </summary>
        /// <param name="args">Ticker Tf [FreeDays]</param>
        public void GetInsStore(List<string> args)
        {
            if (args.Count < 2)
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

            var insStore = _insStoreBL.GetInsStore(ticker, tf);
            if (insStore == null)
            {
                _console.WriteError("Не найден поток данных");
                return;
            }

            _console.WriteLine("InsStoreID\tInsID\tTicker\tTf\tIsEnable");
            _console.WriteSeparator();
            _console.WriteLine(string.Format("{0}\t\t{1}\t{2}\t{3}\t{4}",
                insStore.InsStoreID.ToString(),
                insStore.InsID.ToString(),
                ticker,
                insStore.Tf.ToString(),
                insStore.IsEnable ? "*" : "-"));

            _console.WriteTitle("Periods");
            var calendar = _insStoreBL.GetInsStoreCalendar(insStore.InsStoreID);
            var periods = calendar.Periods.OrderBy(p => p.StartDate).ToList();
            var freeDays = calendar.FreeDays.OrderBy(d => d).ToList();

            foreach (var period in periods)
            {
                _console.WriteLine(string.Format("{0} - {1}",
                    period.StartDate.ToString("dd.MM.yyyy"),
                    period.EndDate.ToString("dd.MM.yyyy") + (period.IsLastDirty ? "-dirty" : "")));
            }

            if (args.Count >= 3 && args[2].Trim().ToLower() == "freedays")
            {
                _console.WriteTitle("Free Days");
                foreach (var day in freeDays)
                {
                    string dow = "";
                    switch (day.DayOfWeek)
                    {
                        case DayOfWeek.Monday: dow = "пн"; break;
                        case DayOfWeek.Tuesday: dow = "вт"; break;
                        case DayOfWeek.Wednesday: dow = "ср"; break;
                        case DayOfWeek.Thursday: dow = "чт"; break;
                        case DayOfWeek.Friday: dow = "пт"; break;
                        case DayOfWeek.Saturday: dow = "сб"; break;
                        case DayOfWeek.Sunday: dow = "вс"; break;
                    }
                    _console.Write(string.Format("{0}-{1}\t", day.ToString("dd.MM.yy"), dow));
                }
                _console.WriteLine("");
            }
        }
    }
}
