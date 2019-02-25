using Common;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Pulxer;
using System;
using System.Collections.Generic;

namespace Cli
{
    public class CmdExecutor : IExecutor
    {
        private readonly IConsole _console;
        private readonly ILogger _logger;
        private readonly IServiceProvider _sp;

        public CmdExecutor(IConsole console, ILogger logger, IServiceProvider sp)
        {
            _console = console;
            _logger = logger;
            _sp = sp;
        }

        public string GetPrefix()
        {
            return "PX";
        }

        public IExecutor Execute(string cmd, List<string> args)
        {
            if (cmd == "help")
            {
                Help();
            }
            else if (cmd == "sync-leech")
            {
                var leechCtrl = ActivatorUtilities.CreateInstance<LeechCtrl>(_sp);
                leechCtrl.SyncLeech();
            }
            else if (cmd == "list-leechinstrum")
            {
                var leechCtrl = ActivatorUtilities.CreateInstance<LeechCtrl>(_sp);
                leechCtrl.ListLeechInstrum();
            }
            else if (cmd == "list-instrum")
            {
                var instrumCtrl = ActivatorUtilities.CreateInstance<InstrumCtrl>(_sp);
                instrumCtrl.ListInstrum();
            }
            else if (cmd == "create-insstores")
            {
                var insStoreCtrl = ActivatorUtilities.CreateInstance<InsStoreCtrl>(_sp);
                insStoreCtrl.CreateInsStore(args);
            }
            else if (cmd == "list-insstore")
            {
                var insStoreCtrl = ActivatorUtilities.CreateInstance<InsStoreCtrl>(_sp);
                insStoreCtrl.ListInsStore();
            }
            else if (cmd == "get-insstore")
            {
                var insStoreCtrl = ActivatorUtilities.CreateInstance<InsStoreCtrl>(_sp);
                insStoreCtrl.GetInsStore(args);
            }
            else if (cmd == "download-all")
            {
                var histCtrl = ActivatorUtilities.CreateInstance<HistoryCtrl>(_sp);
                histCtrl.HistoryDownloadAll(args);
            }
            else if (cmd == "download")
            {
                var histCtrl = ActivatorUtilities.CreateInstance<HistoryCtrl>(_sp);
                histCtrl.HistoryDownloadAsync(args);
            }
            else if (cmd == "get-bars")
            {
                var histCtrl = ActivatorUtilities.CreateInstance<HistoryCtrl>(_sp);
                histCtrl.GetBars(args);
            }
            else if (cmd == "get-tickhistorydates")
            {
                var histCtrl = ActivatorUtilities.CreateInstance<HistoryCtrl>(_sp);
                histCtrl.GetTickHistoryDates(args);
            }
            else if (cmd == "get-tickhistoryinstrums")
            {
                var histCtrl = ActivatorUtilities.CreateInstance<HistoryCtrl>(_sp);
                histCtrl.GetTickHistoryInstrums(args);
            }
            else if (cmd == "list-ticksource")
            {
                var tse = ActivatorUtilities.CreateInstance<TickSourceExecutor>(_sp);
                tse.ListTickSource();
            }
            else if (cmd == "list-testconfig")
            {
                var tce = ActivatorUtilities.CreateInstance<TestConfigExecutor>(_sp);
                tce.ListTestConfig();
            }
            else if (cmd == "testconfig")
            {
                var tce = ActivatorUtilities.CreateInstance<TestConfigExecutor>(_sp);
                int id;
                if (args.Count > 0 && int.TryParse(args[0], out id))
                {
                    tce.InitContext(id, this);
                }
                else
                {
                    tce.InitContext(0, this);
                }
                return tce;
            }
            else if (cmd == "ticksource")
            {
                var tse = ActivatorUtilities.CreateInstance<TickSourceExecutor>(_sp);
                int id;
                if (args.Count > 0 && int.TryParse(args[0], out id))
                {
                    tse.InitContext(id, this);
                }
                else
                {
                    tse.InitContext(0, this);
                }
                return tse;
            }
            else if (cmd == "testrun")
            {
                var tr = ActivatorUtilities.CreateInstance<TestRunCtrl>(_sp);
                tr.TestRunAsync(args);
            }
            else if (cmd == "log")
            {
                Log();
            }
            else if (cmd == "load-trades")
            {
                var posCtrl = ActivatorUtilities.CreateInstance<PositionCtrl>(_sp);
                posCtrl.LoadTrades(args);
            }
            else if (cmd == "refresh-pos")
            {
                var posCtrl = ActivatorUtilities.CreateInstance<PositionCtrl>(_sp);
                posCtrl.RefreshPositions(args);
            }
            else if (cmd == "clear-pos")
            {
                var posCtrl = ActivatorUtilities.CreateInstance<PositionCtrl>(_sp);
                posCtrl.ClearPositions(args);
            }
            else if (cmd == "get-openpos")
            {
                var posCtrl = ActivatorUtilities.CreateInstance<PositionCtrl>(_sp);
                posCtrl.GetPositions(args, true);
            }
            else if (cmd == "get-allpos")
            {
                var posCtrl = ActivatorUtilities.CreateInstance<PositionCtrl>(_sp);
                posCtrl.GetPositions(args, false);
            }
            else if (cmd == "list-user")
            {
                var userCtrl = ActivatorUtilities.CreateInstance<UserCtrl>(_sp);
                userCtrl.ListUsers();
            }
            else if (cmd == "create-user")
            {
                var userCtrl = ActivatorUtilities.CreateInstance<UserCtrl>(_sp);
                userCtrl.CreateUser(args);
            }
            else if (cmd == "set-password")
            {
                var userCtrl = ActivatorUtilities.CreateInstance<UserCtrl>(_sp);
                userCtrl.SetPassword(args);
            }
            else if (cmd == "delete-user")
            {
                var userCtrl = ActivatorUtilities.CreateInstance<UserCtrl>(_sp);
                userCtrl.DeleteUser(args);
            }
            else if (cmd == "file-protect")
            {
                FileProtect(args);
            }
            else
            {
                _console.WriteError("Неизвестная команда");
            }

            return null;
        }

        private void Help()
        {
            _console.WriteLine("Help - Список команд");
            _console.WriteLine("Exit - Выход");
            _console.WriteSeparator();
            _console.WriteLine("Sync-Leech - Синхронизация данных из базы Leech");
            _console.WriteLine("List-LeechInstrum - Список инструментов из базы Leech");
            _console.WriteLine("List-Instrum - Список инструментов из базы Pulxer");
            _console.WriteLine("Create-InsStores Tf Ticker Ticker ... - Создание потоков данных для указанных тикеров");
            _console.WriteLine("List-InsStore - Список потоков данных");
            _console.WriteLine("Get-InsStore Tf Ticker [FreeDays] - Информация по потоку данных");
            _console.WriteLine("Download Date1 Date2 [Dirty] Tf Ticker Ticker ... - Загрузка указанного потока данных (старт фонового процесса), Dirty - последний день неполный");
            _console.WriteLine("Download Progress|Cancel - Показать состояние фонового процесса загрузки | Прервать фоновый процесс");
            _console.WriteLine("Download-All [Full] - Загрузка потоков данных (Full - последний день полный, иначе последний день помечается как Dirty)");
            _console.WriteLine("Download-All Progress|Cancel - Показать состояние фонового процесса загрузки | Прервать фоновый процесс");
            _console.WriteLine("Get-Bars Tf Ticker Date1, Date2 - Вывод исторических данных");
            _console.WriteLine("Get-TickHistoryDates Ticker - Вывод списка дат тиковых исторических данных для инструмента");
            _console.WriteLine("Get-TickHistoryInstrums Date - Вывод списка инструментов тиковых исторических данных для даты");
            _console.WriteLine("TestRun tickSourceID testConfigID - Выполнение тестового прогона (запуск фонового процесса)");
            _console.WriteLine("TestRun Stop - Остановка тестового прогона");
            _console.WriteLine("Load-trades accountID xml-file - Ручная загрузка сделок из xml-файла");
            _console.WriteSeparator();
            _console.WriteLine("Refresh-Pos accountID - Обновление списка позиций по сделкам");
            _console.WriteLine("Clear-Pos accountID - Очистка списка позиций по сделкам");
            _console.WriteLine("Get-OpenPos accountID - Вывод списка открытых позиций");
            _console.WriteLine("Get-AllPos accountID - Вывод полного списка позиций");
            _console.WriteSeparator();
            _console.WriteLine("List-TickSource - Список источников данных");
            _console.WriteLine("List-TestConfig - Список тестовых конфигураций");
            _console.WriteLine("TickSource [Id] - Редактирование тикового источника (если Id не указан, то создание нового)");
            _console.WriteLine("TestConfig [Id] - Редактирование тестовой конфигурации (если Id не указан, то создание новой)");
            _console.WriteSeparator();
            _console.WriteLine("List-User - Список пользователей");
            _console.WriteLine("Create-User Login Password Role - Создание нового пользователя");
            _console.WriteLine("Set-Password Login Password - Смена пароля у пользователя");
            _console.WriteLine("Delete-User Login - Удаление пользователя");
            _console.WriteSeparator();
            _console.WriteLine("File-Protect filename M[achine]|U[ser] - Сформировать новый файл с защищенным содержимым по исходному, M|U - уровень защиты (local machile|current user)");
            _console.WriteSeparator();
            _console.WriteLine("Log - Вывод лога");
        }

        private void Log()
        {
            var logger = _logger as Logger;
            if (logger == null) return;

            var logs = logger.GetLogs();
            foreach (var text in logs)
            {
                _console.WriteLine(text);
            }
        }

        #region FileProtect
        private void FileProtect(List<string> args)
        {
            if (args.Count < 2)
            {
                _console.WriteError("Неверное число аргументов.");
                return;
            }

            bool isLocalMachile = args[1].ToLower().StartsWith('m');

            try
            {
                string resPath = DataProtect.FileProtect(args[0], ".protected", isLocalMachile);
                _console.WriteLine("Сформирован файл:" + resPath);
            }
            catch(Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }
        #endregion
    }
}
