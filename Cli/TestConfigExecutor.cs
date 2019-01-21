using Common;
using Common.Interfaces;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cli
{
    public class TestConfigExecutor : IExecutor
    {
        private IConsole _console = null;
        private IExecutor _parentExecutor = null;
        private TestConfig _testConfig = null;
        private ITestConfigBL _testConfigBL = null;

        public TestConfigExecutor(IConsole console, ITestConfigBL testConfigBL)
        {
            _console = console;
            _testConfigBL = testConfigBL;
        }

        public void InitContext(int id, IExecutor parentExecutor)
        {
            _parentExecutor = parentExecutor;

            if (id > 0)
            {
                _testConfig = _testConfigBL.GetTestConfig(id);
            }
            if (_testConfig == null)
            {
                _testConfig = new TestConfig();
            }

            View();
        }

        public string GetPrefix()
        {
            return "TestConfig";
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
                    _testConfigBL.SaveTestConfig(_testConfig);
                    _console.WriteLine("Сохранено. TestConfigID = " + _testConfig.TestConfigID.ToString());
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
        /// Список тестовых конфигураций
        /// </summary>
        public void ListTestConfig()
        {
            var list = _testConfigBL.GetTestConfigList();

            _console.WriteLine("TestConfigID\tName");
            _console.WriteSeparator();
            foreach (var ts in list)
            {
                _console.WriteLine(string.Format("{0}\t\t{1}",
                    ts.TestConfigID.ToString(),
                    ts.Name));
            }
            _console.WriteLine(string.Format("Test configs: {0}", list.Count().ToString()));
        }

        private void View()
        {
            if (_testConfig == null) return;
            if (_testConfig.TestConfigID <= 0)
            {
                _console.WriteLine("Новая тестовая конфигурация");
            }
            else
            {
                _console.WriteLine("Тестовая конфигурация");
            }
            _console.WriteSeparator();
            string header = string.Format("Name: {0}\nTestConfigID: {1}, Initial Summa: {2}, Commission Percent: {3}%, Short Position: {4}",
                _testConfig.Name,
                _testConfig.TestConfigID.ToString(), 
                _testConfig.InitialSumma.ToString(),
                _testConfig.CommPerc.ToString(),
                _testConfig.IsShortEnable ? "YES" : "NO");
            _console.WriteLine(header);
            _console.WriteTitle("Bot Configs");
            var botConfs = _testConfig.GetBotConfigs();
            foreach (var botConfig in botConfs)
            {
                string item = string.Format("Key = {0}\tAssembly = {1}\tClass = {2}\tInitData = '{3}'", 
                    botConfig.Key, 
                    botConfig.Assembly, 
                    botConfig.Class, 
                    botConfig.InitData);
                _console.WriteLine(item);
            }
        }

        private void Add(List<string> args)
        {
            if (args.Count < 3)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            var key = args[0];
            var assembly = args[1];
            var cls = args[2];
            var initdata = "";

            if (args.Count == 4)
            {
                initdata = args[3];
            }

            _testConfig.AddBotConfig(key, assembly, cls, initdata);
            View();
        }

        private void Remove(List<string> args)
        {
            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            _testConfig.RemoveBotConfig(args[0]);
            View();
        }

        private void Name(List<string> args)
        {
            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            _testConfig.Name = args[0];
            View();
        }

        private void Edit(List<string> args)
        {
            if (args.Count < 3)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            decimal summa = 0;
            if (decimal.TryParse(args[0], out summa))
            {
                _testConfig.InitialSumma = summa;
            }

            decimal comm = 0;
            if (decimal.TryParse(args[1], out comm))
            {
                _testConfig.CommPerc = comm;
            }

            bool se = false;
            if (bool.TryParse(args[2], out se))
            {
                _testConfig.IsShortEnable = se;
            }

            View();
        }

        private void Help()
        {
            _console.WriteLine("Help - Список команд");
            _console.WriteSeparator();
            _console.WriteLine("View - Просмотр");
            _console.WriteLine("Name 'наименование' - Изменение наименования");
            _console.WriteLine("Edit InitialSumma CommPerc IsShortEnable(true/false) - Изменение параметров (в суммах разделитель - запятая)");
            _console.WriteLine("Add Key Assembly Class 'Init data' - Добавить бота");
            _console.WriteLine("Remove Key - Удалить бота");
            _console.WriteSeparator();
            _console.WriteLine("Save - Запись изменений и выход из контекста");
            _console.WriteLine("Cancel - Выход из контекста без записи изменений");
        }
    }
}
