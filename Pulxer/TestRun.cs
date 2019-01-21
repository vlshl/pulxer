using BL;
using Common;
using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Pulxer
{
    public class TestRun
    {
        private TradeEngine _engine = null;
        private TradeEngineData _data = null;
        private TickSource _tickSource = null;
        private readonly IAccountDA _accountDA = null;
        private readonly IInstrumBL _instrumBL = null;
        private readonly IInsStoreBL _insStoreBL = null;
        private readonly ITickSourceBL _tickSourceBL = null;
        private readonly ITestConfigBL _testConfigBL = null;
        private Dictionary<IBot, ILeechPlatform> _bot_platform;
        private BgTaskProgress _progress = null;
        private ILogger _logger = null;

        public TestRun(IAccountDA accountDA, IInstrumBL instrumBL, IInsStoreBL insStoreBL,
            ITickSourceBL tickSourceBL, ITestConfigBL testConfigBL, ILogger logger)
        {
            _accountDA = accountDA;
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _tickSourceBL = tickSourceBL;
            _testConfigBL = testConfigBL;
            _bot_platform = new Dictionary<IBot, ILeechPlatform>();
            _logger = logger;
        }

        public async Task<int> Initialize(int tickSourceID, int testConfigID, BgTaskProgress progress = null)
        {
            _tickSource = _tickSourceBL.GetTickSourceByID(tickSourceID);
            var testConfig = _testConfigBL.GetTestConfig(testConfigID);
            if (_tickSource == null || testConfig == null) return 0;

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
                    var asm = Assembly.LoadFrom(conf.Assembly);
                    if (asm == null)
                        throw new ApplicationException("Сборка не загружена: " + conf.Assembly);

                    var type = asm.GetType(conf.Class);
                    if (type == null)
                        throw new ApplicationException("Тип не найден: " + conf.Class);

                    var platform = new LeechPlatform(_tickSource, _instrumBL, _insStoreBL, _engine, _logger);
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

            var account = _data.GetAccount();
            var cash = _data.GetCash();
            account.AccountType = AccountTypes.Test;
            account.CommPerc = testConfig.CommPerc;
            account.IsShortEnable = testConfig.IsShortEnable;
            account.Name = testConfig.Name + " / " + _tickSource.Name;
            account.Code = "";
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
            }
            else if (state == TestTickSourceState.Stopped)
            {
                _progress.OnProgress(percent);
                _progress.OnAbort();
            }
        }

        private void _tickSource_OnTick(Tick tick)
        {
            _engine.OnTick(tick);
        }

        public void Start()
        {
            if (_tickSource != null) _tickSource.Start();
            if (_progress != null) _progress.OnStart();
        }

        public void Stop()
        {
            if (_tickSource != null) _tickSource.Stop();
            if (_progress != null) _progress.OnAbort();
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
