using Common;
using Common.Interfaces;
using Platform;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cli
{
    public class TickSourceExecutor : IExecutor
    {
        private IConsole _console = null;
        private IExecutor _parentExecutor = null;
        private TickSource _tickSource = null;
        private readonly ITickSourceBL _tickSourceBL = null;
        private readonly IInstrumBL _instrumBL = null;
        private readonly IInsStoreBL _insStoreBL = null;

        public TickSourceExecutor(IConsole console, ITickSourceBL tickSourceBL, IInstrumBL instrumBL, IInsStoreBL insStoreBL)
        {
            _console = console;
            _tickSourceBL = tickSourceBL;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
        }

        public void InitContext(int id, IExecutor parentExecutor)
        {
            _parentExecutor = parentExecutor;

            if (id > 0)
            {
                _tickSource = _tickSourceBL.GetTickSourceByID(id);
            }
            if (_tickSource == null)
            {
                _tickSource = new TickSource(_instrumBL, _insStoreBL, null);
            }

            View();
        }

        public string GetPrefix()
        {
            return "TickSource";
        }

        public IExecutor Execute(string cmd, List<string> args)
        {
            if (cmd == "help")
            {
                Help();
            }
            else if (cmd == "view")
            {
                View();
            }
            else if (cmd == "add")
            {
                Add(args);
            }
            else if (cmd == "remove")
            {
                Remove(args);
            }
            else if (cmd == "name")
            {
                Name(args);
            }
            else if (cmd == "edit")
            {
                Edit(args);
            }
            else if (cmd == "save")
            {
                try
                {
                    _tickSourceBL.SaveTickSource(_tickSource);
                    _console.WriteLine("Сохранено. TickSourceID = " + _tickSource.TickSourceID.ToString());
                    return _parentExecutor;
                }
                catch (Exception ex)
                {
                    _console.WriteError(ex.ToString());
                }
            }
            else if (cmd == "cancel")
            {
                return _parentExecutor;
            }
            else
            {
                _console.WriteError("Неизвестная команда");
            }

            return null;
        }

        /// <summary>
        /// Список тиковых источников
        /// </summary>
        public void ListTickSource()
        {
            var list = _tickSourceBL.GetTickSourceList();

            _console.WriteLine("TickSourceID\tName");
            _console.WriteSeparator();
            foreach (var ts in list)
            {
                _console.WriteLine(string.Format("{0}\t\t{1}",
                    ts.TickSourceID.ToString(),
                    ts.Name));
            }
            _console.WriteLine(string.Format("Tick sources: {0}", list.Count().ToString()));
        }

        private void View()
        {
            if (_tickSource == null) return;
            if (_tickSource.TickSourceID <= 0)
            {
                _console.WriteLine("Новый тиковый источник");
            }
            else
            {
                _console.WriteLine("Тиковый источник");
            }
            _console.WriteSeparator();
            string header = string.Format("Name: {0}\nTickSourceID: {1}, Start Date: {2}, End Date: {3}, Timeframe: {4}",
                _tickSource.Name,
                _tickSource.TickSourceID.ToString(),
                _tickSource.StartDate.ToString("dd.MM.yyyy"),
                _tickSource.EndDate.ToString("dd.MM.yyyy"),
                _tickSource.Timeframe.ToString());
            _console.WriteLine(header);
            _console.WriteTitle("Instrums");
            var instrums = _tickSource.GetInstrums();
            foreach (var instrum in instrums)
            {
                string item = string.Format("Ticker = {0}\tShortName = {1}",
                    instrum.Ticker,
                    instrum.ShortName);
                _console.WriteLine(item);
            }
        }

        private void Add(List<string> args)
        {
            if (args.Count == 0)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            foreach (var ticker in args)
            {
                var isSuccess = _tickSource.AddInstrum(ticker);
                if (isSuccess)
                    _console.WriteLine("Добавлен: " + ticker);
                else
                    _console.WriteError("Неверный тикер: " + ticker);
            }
            View();
        }

        private void Remove(List<string> args)
        {
            if (args.Count == 0)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            foreach (var ticker in args)
            {
                _tickSource.RemoveInstrum(ticker);
            }
            View();
        }

        private void Name(List<string> args)
        {
            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            _tickSource.Name = args[0];
            View();
        }

        private void Edit(List<string> args)
        {
            if (args.Count < 3)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            DateTime start;
            if (DateTime.TryParse(args[0], out start))
            {
                _tickSource.StartDate = start;
            }

            DateTime end;
            if (DateTime.TryParse(args[1], out end))
            {
                _tickSource.EndDate = end;
            }

            Timeframes tf;
            if (Timeframes.TryParse(args[2], out tf))
            {
                _tickSource.Timeframe = tf;
            }

            View();
        }

        private void Help()
        {
            _console.WriteLine("Help - Список команд");
            _console.WriteSeparator();
            _console.WriteLine("View - Просмотр");
            _console.WriteLine("Name 'наименование' - Изменение наименования");
            _console.WriteLine("Edit StartDate EndDate Timeframe - Изменение параметров");
            _console.WriteLine("Add Ticker Ticker ... - Добавить инструменты");
            _console.WriteLine("Remove Ticker Ticker ... - Удалить инструменты");
            _console.WriteSeparator();
            _console.WriteLine("Save - Запись изменений и выход из контекста");
            _console.WriteLine("Cancel - Выход из контекста без записи изменений");
        }
    }
}
