using Common;
using Common.Interfaces;
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
    public class TickDispatcher : ITickDispatcher
    {
        //private ITickSource _tickSource = null;
        private List<TickThreadControl> _ttcs = null;
        private Dictionary<int, List<Tick>> _insID_ticks;
        private readonly ILogger _logger = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        public TickDispatcher(ILogger logger)
        {
            _logger = logger;
            _ttcs = new List<TickThreadControl>();
            _insID_ticks = new Dictionary<int, List<Tick>>();
        }

        /// <summary>
        /// Инициализация перед началом торговой сессии
        /// </summary>
        public void Initialize()
        {
            _logger.AddInfo("TickDispatcher", "Initialize ...");
            _ttcs.Clear();
            _insID_ticks.Clear();
            _logger.AddInfo("TickDispatcher", "Initialized");
        }

        /// <summary>
        /// Завершение и очистка после окончания торговой сессии
        /// </summary>
        public void Close()
        {
            _logger.AddInfo("TickDispatcher", "Close ...");
            UnsubscribeAll();
            _insID_ticks.Clear();
            _logger.AddInfo("TickDispatcher", "Closed");
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
            var ticks = GetTickList(insID);
            TickThreadControl ttc = new TickThreadControl(subscriber, insID, ticks);
            ttc.IsRunning = true;
            CreateTickThread(onTick, ttc);
            _ttcs.Add(ttc);
        }

        /// <summary>
        /// Отписаться от потока сделок.
        /// Отписываются все подписчики указанного инструмента.
        /// </summary>
        /// <param name="subscriber">Объект-подписчик</param>
        /// <param name="insID">Инструмент</param>
        public void Unsubscribe(object subscriber, int insID)
        {
            var list = _ttcs.Where(t => object.ReferenceEquals(t.Subscriber, subscriber) 
                && t.InsID == insID).ToList();
            foreach (var r in list)
            {
                r.IsRunning = false;
                r.Mre.Set();
                _ttcs.Remove(r);
            }
        }

        /// <summary>
        /// Отписать всех подписчиков
        /// </summary>
        public void UnsubscribeAll()
        {
            var list = _ttcs.ToList();
            foreach (var r in list)
            {
                r.IsRunning = false;
                r.Mre.Set();
                _ttcs.Remove(r);
            }
        }

        public DateTime CurrentDate
        {
            get
            {
                return DateTime.Today; // ???????????????????
            }
        }

        #endregion

        /// <summary>
        /// Явное добавление новой сделки
        /// </summary>
        /// <param name="tick">Информация по сделке</param>
        public void AddTick(Tick tick)
        {
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
        /// Последний тик по инструменту
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns>Если тиков нет, то default(Tick)</returns>
        public Tick GetLastTick(int insID)
        {
            var ticks = GetTickList(insID);
            lock (ticks)
            {
                return ticks.LastOrDefault();
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
            if (!_insID_ticks.ContainsKey(insID)) _insID_ticks.Add(insID, new List<Tick>());
            return _insID_ticks[insID];
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
