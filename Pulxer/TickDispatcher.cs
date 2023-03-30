using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using Pulxer.Leech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer
{
    /// <summary>
    /// Диспетчер потока сделок.
    /// Принимает поток сделок по всем инструментам.
    /// Ведет список подписчиков на потоки сделок по указанным инструментам и распределяет потоки по подписчикам.
    /// Накапливает информацию по сделкам, чтобы выдавать подписчикам не только актуальные данные, но и те данные, 
    /// которые подписчик еще не успел получить, если поздно подписался на поток.
    /// То есть если торговая сессия уже идет, то новый подписчик сначала получит все сделки с начала торговой сессии, 
    /// а затем начнет получать актуальные сделки по мере их возникновения.
    /// </summary>
    public class TickDispatcher : ITickSubscribe
    {
        //private ITickSource _tickSource = null;
        private List<TickThreadControl> _ttcs = null;
        private Dictionary<int, List<Tick>> _insID_ticks;
        private readonly ILogger _logger = null;
        private Tick _lastTick;
        private DateTime _sessionDate;
        private bool _isInitializing = false;
        private TickProvider _tickProvider = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        public TickDispatcher(ILogger<TickDispatcher> logger)
        {
            _logger = logger;
            _ttcs = new List<TickThreadControl>();
            _insID_ticks = new Dictionary<int, List<Tick>>();
            _lastTick = default;
            _sessionDate = DateTime.Today;
        }

        /// <summary>
        /// Инициализация перед началом торговой сессии
        /// </summary>
        /// <param name="sessionDate">Дата новой торговой сессии</param>
        public void Initialize(DateTime sessionDate)
        {
            lock (_insID_ticks)
            {
                if (_isInitializing) return;

                _logger.LogInformation("Initialize ...");
                _isInitializing = true;
                UnsubscribeAll();
                _insID_ticks.Clear();
                _sessionDate = sessionDate;
                _isInitializing = false;
                _logger.LogInformation("Initialized");
            }
        }

        public void SetTickProvider(TickProvider tp)
        {
            _tickProvider = tp;

            // в момент вызова SetTickProvider диспетчер TickDispatcher уже создан и возможно уже имеет подписки Subscribe
            // поэтому сообщаем провайдеру все инструменты, на которые уже есть подписки и для которых надо получать тиковые данные
            lock (_insID_ticks)
            {
                foreach(int insId in _insID_ticks.Keys)
                {
                    _tickProvider.AddInstrum(insId);
                }
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="ts">Источник всех сделок по всем инструментам</param>
        //public TickDispatcher(ITickSource ts) : this()
        //{
        //    _tickSource = ts;
        //    _tickSource.OnTick += AddTick;
        //    _tickSource.OnChangeDate += _tickSource_OnChangeDate;
        //}

        #region ITickDispatcher
        /// <summary>
        /// Добавить подписчика на поток сделок по указанному инструменту.
        /// Для одного инструмента можно создавать несколько подписчиков.
        /// </summary>
        /// <param name="subscriber">Объект-подписчик</param>
        /// <param name="insID">Инструмент</param>
        /// <param name="onTick">Callback, который будет вызываться диспетчером при каждой сделке указанного инструмента</param>
        public void Subscribe(object subscriber, int insID, OnTickEH onTick)
        {
            if (_isInitializing) return;

            var ticks = GetTickList(insID);
            TickThreadControl ttc = new TickThreadControl(subscriber, insID, ticks);
            ttc.IsRunning = true;
            CreateTickThread(onTick, ttc);
            _ttcs.Add(ttc);
            _tickProvider?.AddInstrum(insID);
        }

        /// <summary>
        /// Отписаться от потока сделок.
        /// Если у инструмента и подписчика было несколько подписок (т.е. Subscribe вызывался несколько раз с одними и теми же subscriber и insID), удаляются все подписки.
        /// </summary>
        /// <param name="subscriber">Объект-подписчик</param>
        /// <param name="insID">Инструмент</param>
        public void Unsubscribe(object subscriber, int insID)
        {
            if (_isInitializing) return;

            var list = _ttcs.Where(t => object.ReferenceEquals(t.Subscriber, subscriber) 
                && t.InsID == insID).ToList();
            foreach (var r in list)
            {
                r.IsRunning = false;
                r.Mre.Set();
                _ttcs.Remove(r);
                _tickProvider?.RemoveInstrum(insID);
            }
        }

        /// <summary>
        /// Отписать всех подписчиков
        /// </summary>
        public void UnsubscribeAll()
        {
            if (_isInitializing) return;

            var list = _ttcs.ToList();
            foreach (var r in list)
            {
                r.IsRunning = false;
                r.Mre.Set();
                _ttcs.Remove(r);
                _tickProvider?.RemoveInstrum(r.InsID);
            }
        }

        public DateTime SessionDate
        {
            get
            {
                return _sessionDate;
            }
        }
        #endregion

        /// <summary>
        /// Явное добавление новой сделки
        /// </summary>
        /// <param name="tick">Информация по сделке</param>
        public void AddTick(Tick tick)
        {
            if (_isInitializing) return;
            if (tick.Time.Date != _sessionDate) return;

            lock (this)
            {
                _lastTick = tick;
            }

            var ticks = GetTickList(tick.InsID);
            lock (ticks)
            {
                ticks.Add(tick);
            }
            var list = _ttcs.Where(t => t.InsID == tick.InsID).ToList();
            foreach (var r in list)
            {
                r.Mre.Set();
            }
        }

        /// <summary>
        /// Явное добавление новых сделок по одному инструменту.
        /// Дата проверяется на соответствие SessionDate только у последнего тика.
        /// </summary>
        /// <param name="ticks">Информация по сделкам (у всех сделок должен быть один инструмент)</param>
        public void AddTicks(IEnumerable<Tick> ticks)
        {
            if (_isInitializing) return;
            if (!ticks.Any()) return;
            if (ticks.Last().Time.Date != _sessionDate) return;

            lock (this)
            {
                _lastTick = ticks.Last();
            }

            int insId = ticks.First().InsID;
            var tickList = GetTickList(insId);
            lock (tickList)
            {
                tickList.AddRange(ticks);
            }
            var list = _ttcs.Where(t => t.InsID == insId).ToList();
            foreach (var r in list)
            {
                r.Mre.Set();
            }
        }

        /// <summary>
        /// Последний тик по инструменту
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns>Если тиков нет, то default(Tick)</returns>
        public Tick GetLastTick(int insID)
        {
            if (_isInitializing) return default(Tick);

            var ticks = GetTickList(insID);
            lock (ticks)
            {
                return ticks.LastOrDefault();
            }
        }

        /// <summary>
        /// Последний тик
        /// </summary>
        /// <returns>Если тиков нет, то default(Tick)</returns>
        public Tick GetLastTick()
        {
            if (_isInitializing) return default(Tick);

            lock (this)
            {
                return _lastTick;
            }
        }

        private void CreateTickThread(OnTickEH onTick, TickThreadControl ttc)
        {
            Task.Factory.StartNew(() =>
            {
                Tick? tick = null;
                while (ttc.IsRunning)
                {
                    lock (ttc.Ticks)
                    {
                        if (ttc.Index < ttc.Ticks.Count)
                        {
                            tick = ttc.Ticks[ttc.Index++];
                        }
                        else
                        {
                            tick = null;
                            ttc.Mre.Reset();
                        }
                    }
                    ttc.Mre.WaitOne();
                    if (tick != null)
                    {
                        onTick(tick.Value);
                    }
                }
            });
        }

        private List<Tick> GetTickList(int insID)
        {
            lock (_insID_ticks)
            {
                if (!_insID_ticks.ContainsKey(insID)) _insID_ticks.Add(insID, new List<Tick>());
                return _insID_ticks[insID];
            }
        }

        /// <summary>
        /// Список инструментов, по которым накоплены сделки
        /// </summary>
        public IEnumerable<int> GetInstrumIDs()
        {
            lock (_insID_ticks)
            {
                return _insID_ticks.Keys.ToList();
            }
        }

        /// <summary>
        /// Список накопленных сделок
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns></returns>
        public IEnumerable<Tick> GetTicks(int insID)
        {
            lock (_insID_ticks)
            {
                if (!_insID_ticks.ContainsKey(insID)) return null;
                return _insID_ticks[insID].ToList();
            }
        }

        /// <summary>
        /// Количество накопленных сделок на данный момент
        /// </summary>
        /// <param name="insId">Инструмент</param>
        /// <returns>Количество сделок</returns>
        public int GetTicksCount(int insId)
        {
            lock (_insID_ticks)
            {
                if (!_insID_ticks.ContainsKey(insId)) return 0;
                return _insID_ticks[insId].Count();
            }
        }

        //void _tickSource_OnChangeDate(DateTime date)
        //{
        //    foreach (var ttc in _ttcs)
        //    {
        //        ttc.Reset();
        //    }
        //}
    }

    /// <summary>
    /// Объект, управляющий каждым подписчиком
    /// </summary>
    internal class TickThreadControl
    {
        public object Subscriber = null;
        public int InsID = 0;
        public bool IsRunning = false;
        public List<Tick> Ticks = null;
        public ManualResetEvent Mre = null;
        public int Index = 0;

        public TickThreadControl(object subscriber, int insID, List<Tick> ticks)
        {
            this.Subscriber = subscriber;
            this.InsID = insID;
            this.Ticks = ticks;
            IsRunning = false;
            Mre = new ManualResetEvent(true);
        }

        public void Reset()
        {
            lock (Ticks)
            {
                Index = 0;
                Ticks.Clear();
            }
        }
    }
}
