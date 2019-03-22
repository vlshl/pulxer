using BL;
using Common;
using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer
{
    public delegate void TestRunFinished(bool isComplete);

    public class TestRun
    {
        private TradeEngine _engine;
        private TradeEngineData _data;
        private TickSource _tickSource;
        private readonly IAccountBL _accountBL;
        private readonly IAccountDA _accountDA;
        private readonly IInstrumBL _instrumBL;
        private readonly IInsStoreBL _insStoreBL;
        private readonly ITickSourceBL _tickSourceBL;
        private readonly ITestConfigBL _testConfigBL;
        private Dictionary<IBot, ILeechPlatform> _bot_platform;
        private BgTaskProgress _progress;
        private ILogger _logger;
        private IConfig _config;
        private TestRunFinished _finished;

        public TestRun(IAccountBL accountBL, IAccountDA accountDA, IInstrumBL instrumBL, IInsStoreBL insStoreBL,
            ITickSourceBL tickSourceBL, ITestConfigBL testConfigBL, ILogger logger, IConfig config)
        {
            _accountBL = accountBL;
            _accountDA = accountDA;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _tickSourceBL = tickSourceBL;
            _testConfigBL = testConfigBL;
            _bot_platform = new Dictionary<IBot, ILeechPlatform>();
            _logger = logger;
            _config = config;
        }

        /// <summary>
        /// Инициализация объекта Тестовый прогон
        /// </summary>
        /// <param name="tickSourceID">Источник тиковых данных</param>
        /// <param name="testConfigID">Тестовая конфигурация</param>
        /// <param name="accountID">Торговый счет, если не указан, то создается новый, иначе берется указанный, в нем очищаются все данные и он заполняется новыми данными</param>
        /// <param name="progress">Индикатор прогресса</param>
        /// <returns>Количество загруженных тиков, на которых будет выполняться тестирование</returns>
        public async Task<int> Initialize(int tickSourceID, int testConfigID, int? accountID, BgTaskProgress progress = null)
        {
            _tickSource = _tickSourceBL.GetTickSourceByID(tickSourceID);
            var testConfig = _testConfigBL.GetTestConfig(testConfigID);
            if (_tickSource == null || testConfig == null) return 0;

            Account account = null;
            if (accountID != null)
            {
                account = _accountBL.GetAccountByID(accountID.Value);
                if (account == null)
                    throw new ApplicationException("Указанный счет не найден.");
                if (account.AccountType != AccountTypes.Test)
                    throw new ApplicationException("Указанный счет не может использоваться для тестирования.");
            }

            _progress = progress;
            _data = new TradeEngineData(_accountDA);
            _engine = new TradeEngine(_data, _instrumBL, (ITimeProvider)_tickSource);
            _tickSource.OnTick += _tickSource_OnTick;
            _tickSource.OnStateChange += _tickSource_OnStateChange;

            var botConfigs = testConfig.GetBotConfigs();
            foreach (var conf in botConfigs)
            {
                try
                {
                    string asmPath = "";
                    if (Path.IsPathRooted(conf.Assembly))
                    {
                        asmPath = conf.Assembly;
                    }
                    else
                    {
                        string botsPath = _config.GetBotsPath();
                        asmPath = Path.Combine(botsPath, conf.Assembly);
                    }

                    var asm = Assembly.LoadFrom(asmPath);
                    if (asm == null)
                        throw new ApplicationException("Сборка не загружена: " + asmPath);

                    var type = asm.GetType(conf.Class);
                    if (type == null)
                        throw new ApplicationException("Тип не найден: " + conf.Class);

                    var platform = new LeechPlatform(_tickSource, _instrumBL, _insStoreBL, _engine, _data, _logger);
                    var bot = Activator.CreateInstance(type, platform) as IBot;
                    if (bot == null)
                        throw new ApplicationException("Бот не создан: " + conf.Key);

                    bot.Initialize(conf.InitData);

                    // после успешной инициализации бота
                    _bot_platform.Add(bot, platform);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Ошибка при инициализации ботов", ex);
                }
            }

            if (account != null)
            {
                _accountBL.DeleteTestAccountData(account.AccountID, false);
                _data.LoadData(account.AccountID);
            }

            var acc = _data.GetAccount();
            var cash = _data.GetCash();
            acc.AccountType = AccountTypes.Test;
            acc.CommPerc = testConfig.CommPerc;
            acc.IsShortEnable = testConfig.IsShortEnable;
            acc.Name = testConfig.Name + " - " + _tickSource.Name;
            acc.Code = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            cash.Initial = cash.Current = testConfig.InitialSumma;

            return await _tickSource.LoadDataAsync();
        }

        private void _tickSource_OnStateChange(TestTickSourceState state, int countTicks, int totalTicks)
        {
            if (_progress == null) return;

            double percent;
            if (totalTicks != 0)
                percent = (double)countTicks / totalTicks * 100;
            else
                percent = 100;

            if (state == TestTickSourceState.Running)
            {
                _progress.OnProgress(percent);
            }
            else if (state == TestTickSourceState.Completed)
            {
                _progress.OnProgress(percent);
                _progress.OnComplete();
                _finished?.Invoke(true);
            }
            else if (state == TestTickSourceState.Stopped)
            {
                _progress.OnProgress(percent);
                _progress.OnAbort();
                _finished?.Invoke(false);
            }
        }

        private void _tickSource_OnTick(Tick tick)
        {
            _engine.OnTick(tick);
        }

        public void Start(TestRunFinished finished)
        {
            _finished = finished;
            if (_tickSource != null) _tickSource.Start();
            if (_progress != null) _progress.OnStart();
        }

        public void Stop()
        {
            if (_tickSource != null) _tickSource.Stop();
            if (_progress != null) _progress.OnAbort();
        }

        public TradeEngineData GetData()
        {
            return _data;
        }

        public void Close()
        {
            if (_tickSource != null)
            {
                _tickSource.OnTick -= _tickSource_OnTick;
                _tickSource.OnStateChange -= _tickSource_OnStateChange;
            }

            foreach (var bot in _bot_platform.Keys)
            {
                bot.Close();
                _bot_platform[bot].Close();
            }
        }
    }
}
