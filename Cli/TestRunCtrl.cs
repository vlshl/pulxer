using BL;
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
        private readonly IAccountBL _accountBL;
        private readonly IInstrumBL _instrumBL; 
        private readonly IInsStoreBL _insStoreBL; 
        private readonly ITickSourceBL _tickSourceBL; 
        private readonly ITestConfigBL _testConfigBL;
        private readonly ILogger _logger;
        private readonly IConfig _config;
        private readonly IPositionBL _posBL;
        private readonly IRepositoryBL _reposBL;

        public TestRunCtrl(IConsole console, IAccountDA accountDA, IAccountBL accountBL, IInstrumBL instrumBL, IInsStoreBL insStoreBL, ITickSourceBL tickSourceBL, 
            ITestConfigBL testConfigBL, ILogger logger, IConfig config, IPositionBL posBL, IRepositoryBL reposBL) : base(console)
        {
            _console = console;
            _accountDA = accountDA;
            _accountBL = accountBL;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _tickSourceBL = tickSourceBL;
            _testConfigBL = testConfigBL;
            _logger = logger;
            _config = config;
            _posBL = posBL;
            _reposBL = reposBL;
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
            int? accountID = null;

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

            if (args.Count >= 3)
            {
                if (int.TryParse(args[2].Trim(), out r))
                {
                    accountID = r;
                }
                else
                {
                    _console.WriteError("Неверно указан id счета");
                    return;
                }
            }

            _console.WriteLine("Загрузка данных ... ");
            _testRun = new TestRun(_accountBL, _accountDA, _instrumBL, _insStoreBL, _tickSourceBL, _testConfigBL, _logger, _config, _posBL, _reposBL);
            _progress = new BgTaskProgress(_syncContext, "Тестовый прогон");

            try
            {
                bool isSuccess = await _testRun.Initialize(tickSourceID, testConfigID, accountID, _progress);
                if (isSuccess)
                {
                    var stat = _testRun.GetTickSourceStatistics();

                    _console.WriteLine(string.Format("Всего загружено дней: {0}, тиков: {1}", stat.TotalDaysCount.ToString(), stat.TotalTicksCount.ToString()));
                    _console.WriteLine(string.Format("Из них синтезировано дней: {0} ({1}%), тиков: {2} ({3}%)",
                        stat.SynDaysCount.ToString(),
                        (stat.TotalDaysCount != 0 ? (decimal)stat.SynDaysCount * 100 / stat.TotalDaysCount : 0).ToString("##0.0#"),
                        stat.SynTicksCount,
                        (stat.TotalTicksCount != 0 ? (decimal)stat.SynTicksCount * 100 / stat.TotalTicksCount : 0).ToString("##0.0#")));
                    _console.WriteLine("Тестовый прогон выполняется ... ");

                    _testRun.Start(TestRunFinished);
                }
                else
                {
                    _console.WriteLine("Ошибка при инициализации.");
                }
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        private void TestRunFinished(bool isComplete)
        {
            try
            {
                _testRun.Close();
            }
            catch (Exception ex)
            {
                _console.WriteError("Ошибка при завершении тестового прогона.\n" + ex.ToString());
            }

            if (!isComplete)
            {
                _console.WriteLine("Тестовый прогон прерван");
                return;
            }

            _console.WriteLine("Тестовый прогон завершен");
            var data = _testRun.GetData();
            ViewData(data);
        }

        private void ViewData(TradeEngineData data)
        {
            var acc = data.GetAccount();
            var cash = data.GetCash();

            var accStr = string.Format("Торговый счет: Id = {0}\nCode:{1}\nName:{2}", 
                acc.AccountID.ToString(), acc.Code, acc.Name);
            _console.WriteLine(accStr);

            decimal init = cash.Initial;
            decimal curr = cash.Current;

            var cashStr = string.Format("Cash initial={0}, current={1}", 
                init.ToString(), curr.ToString());
            _console.WriteLine(cashStr);

            if (init != 0)
            {
                var profitStr = string.Format("Profit={0} ({1} %)",
                    (curr - init).ToString(), ((curr - init) / init * 100m).ToString());
                _console.WriteLine(profitStr);
            }
        }
    }
}
