using Common.Interfaces;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
    public class TestRunCtrl : BgCtrl
    {
        private TestRun _testRun = null;
        private readonly IConsole _console;
        private readonly IAccountDA _accountDA; 
        private readonly IInstrumBL _instrumBL; 
        private readonly IInsStoreBL _insStoreBL; 
        private readonly ITickSourceBL _tickSourceBL; 
        private readonly ITestConfigBL _testConfigBL;
        private readonly ILogger _logger;

        public TestRunCtrl(IConsole console, IAccountDA accountDA, IInstrumBL instrumBL, IInsStoreBL insStoreBL, ITickSourceBL tickSourceBL, 
            ITestConfigBL testConfigBL, ILogger logger) : base(console)
        {
            _console = console;
            _accountDA = accountDA;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _tickSourceBL = tickSourceBL;
            _testConfigBL = testConfigBL;
            _logger = logger;
        }

        public async void TestRunAsync(List<string> args)
        {
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
                else if (args[0].Trim().ToLower() == "stop")
                {
                    if (_testRun != null)
                    {
                        _testRun.Stop();
                        _console.WriteLine("Тестовый прогон остановлен");
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
                _console.WriteError("Неверное число аргументов");
                return;
            }

            int tickSourceID;
            int testConfigID;
            int r;
            if (int.TryParse(args[0].Trim(), out r))
            {
                tickSourceID = r;
            }
            else
            {
                _console.WriteError("Неверно указан id источника");
                return;
            }
            if (int.TryParse(args[1].Trim(), out r))
            {
                testConfigID = r;
            }
            else
            {
                _console.WriteError("Неверно указан id тестовой конфигурации");
                return;
            }

            _console.WriteLine("Загрузка данных ... ");
            _testRun = new TestRun(_accountDA, _instrumBL, _insStoreBL, _tickSourceBL, _testConfigBL, _logger);
            _progress = new BgTaskProgress(_syncContext, "Тестовый прогон");

            try
            {
                int count = await _testRun.Initialize(tickSourceID, testConfigID, _progress);
                _console.WriteLine("Загружено: " + count.ToString());
                _console.Write("Тестовый прогон выполняется ... ");
                _testRun.Start();
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }
    }
}
