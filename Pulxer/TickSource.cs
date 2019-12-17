using Common;
using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Pulxer
{
    /// <summary>
    /// TickSource
    /// </summary>
    public class TickSource : ITickSource, ITimeProvider
    {
        private readonly IInstrumBL _instrumBL;
        private readonly IInsStoreBL _insStoreBL;
        private readonly ITickHistoryBL _tickHistoryBL;
        private ISyncContext _syncContext = null;

        private List<Instrum> _instrums;
        private List<Tick> _ticks;
        private int _synTicksCount;
        private int _realDays;
        private int _synDays;
        private DateTime? _curTime = null;
        private object _curTimeLock = new object();
        private Dictionary<int, Tick> _insID_lastTick;

        public TickSource(IInstrumBL instrumBL, IInsStoreBL insStoreBL, ITickHistoryBL tickHistoryBL, ISyncContext syncContext = null)
        {
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _tickHistoryBL = tickHistoryBL;
            _syncContext = syncContext;

            TickSourceID = 0;
            Name = "";
            _instrums = new List<Instrum>();
            _ticks = new List<Tick>();
            _synTicksCount = 0;
            _realDays = _synDays = 0;
            _insID_lastTick = new Dictionary<int, Tick>();
        }

        public int TickSourceID { get; set; }

        public string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public Timeframes Timeframe { get; set; }

        public ISyncContext SyncContext
        {
            get
            {
                return _syncContext;
            }
            set
            {
                _syncContext = value;
            }
        }

        public bool AddInstrum(int instrumID)
        {
            var instrum = _instrumBL.GetInstrumByID(instrumID);
            if (instrum == null) return false;

            if (_instrums.Any(r => r.InsID == instrumID)) return false;

            _instrums.Add(instrum);

            return true;
        }

        public bool AddInstrum(string ticker)
        {
            var instrum = _instrumBL.GetInstrum(ticker);
            if (instrum == null) return false;

            if (_instrums.Any(r => r.InsID == instrum.InsID)) return false;

            _instrums.Add(instrum);

            return true;
        }

        public void RemoveInstrum(int instrumID)
        {
            var found = _instrums.FirstOrDefault(r => r.InsID == instrumID);
            if (found != null) _instrums.Remove(found);
        }

        public void RemoveInstrum(string ticker)
        {
            var found = _instrums.FirstOrDefault(r => r.Ticker == ticker);
            if (found != null) _instrums.Remove(found);
        }

        public IEnumerable<Instrum> GetInstrums()
        {
            return _instrums.ToList();
        }

        /// <summary>
        /// Получить последний тик для инструмента для определения последней цены
        /// </summary>
        /// <param name="insID">Инструмент</param>
        /// <returns>Последний тик. Если не найден, то возвращается null.</returns>
        public Tick? GetLastTick(int insID)
        {
            if (!_insID_lastTick.ContainsKey(insID)) return null;
            return _insID_lastTick[insID];
        }

        public void Initialize(XDocument xDoc)
        {
            var xnRoot = xDoc.Element("TickSource");
            if (xnRoot == null) return;

            try
            {
                var start = xnRoot.Attribute("Start").Value;
                StartDate = new DateTime(long.Parse(start));

                var end = xnRoot.Attribute("End").Value;
                EndDate = new DateTime(long.Parse(end));

                var tf = xnRoot.Attribute("Tf").Value;
                Timeframe = (Timeframes)int.Parse(tf);

                _instrums.Clear();
                var ins = xnRoot.Attribute("InsIDs").Value;
                var ids = ins.Split(',');
                foreach (var id in ids)
                {
                    if (string.IsNullOrEmpty(id)) continue;
                    int insID = int.Parse(id);
                    AddInstrum(insID);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при создании TickSource", ex);
            }
        }

        public XDocument Serialize()
        {
            XDocument xd = new XDocument(new XElement("TickSource"));
            xd.Root.Add(new XAttribute("Start", StartDate.Ticks.ToString()));
            xd.Root.Add(new XAttribute("End", EndDate.Ticks.ToString()));
            xd.Root.Add(new XAttribute("Tf", ((int)Timeframe).ToString()));

            var ids = _instrums.Select(r => r.InsID.ToString()).ToList();
            xd.Root.Add(new XAttribute("InsIDs", string.Join(',', ids)));

            return xd;
        }

        /// <summary>
        /// Загрузка тиковых данных для всех инструментов
        /// </summary>
        /// <returns>Общее кол-во загруженных тиков</returns>
        public async Task<int> LoadDataAsync()
        {
            lock (_ticks)
            {
                _ticks.Clear();
            }
            _synTicksCount = _realDays = _synDays = 0;
            _insID_lastTick.Clear();

            foreach (var instrum in _instrums)
            {
                if (Timeframe != Timeframes.Tick)
                {
                    var barRow = new BarRow(Timeframe, instrum.InsID);
                    await _insStoreBL.LoadHistoryAsync(barRow, instrum.InsID, StartDate, EndDate);

                    await Task.Run(() =>
                    {
                        foreach (var bar in barRow.Bars)
                        {
                            lock (_ticks)
                            {
                                int v = bar.Volume < int.MaxValue ? (int)bar.Volume : int.MaxValue;
                                _ticks.Add(new Tick(0, bar.Time, instrum.InsID, 0, bar.Open));
                                _ticks.Add(new Tick(0, bar.Time, instrum.InsID, 0, bar.High));
                                _ticks.Add(new Tick(0, bar.Time, instrum.InsID, 0, bar.Low));
                                _ticks.Add(new Tick(0, bar.Time, instrum.InsID, v, bar.Close));
                            }
                        }
                    });
                }
                else
                {
                    List<DateTime> freeDays = null;
                    var minInsStore = _insStoreBL.GetInsStore(instrum.InsID, Timeframes.Min);
                    if (minInsStore != null)
                    {
                        freeDays = _insStoreBL.GetInsStoreCalendar(minInsStore.InsStoreID).FreeDays
                            .Where(d => d >= StartDate && d <= EndDate).ToList();
                    }

                    DateTime date = StartDate;
                    while (date <= EndDate)
                    {
                        var ticks = await _tickHistoryBL.GetTicksAsync(instrum.InsID, date);
                        if (ticks != null && ticks.Any())
                        {
                            lock (_ticks)
                            {
                                _ticks.AddRange(ticks);
                            }
                            _realDays++;
                        }
                        else // тиковых данных нет, попробуем загрузить минутки и сделать из них тики
                        {
                            if (freeDays != null && !freeDays.Contains(date)) // дата не является выходным днем, значит должны быть минутки
                            {
                                BarRow barRow = new BarRow(Timeframes.Min, instrum.InsID);
                                await _insStoreBL.LoadHistoryAsync(barRow, instrum.InsID, date, date, minInsStore.InsStoreID);
                                if (barRow.Bars.Any())
                                {
                                    foreach (Bar bar in barRow.Bars)
                                    {
                                        var synTicks = SynTicks(bar, instrum); // синтезируем тики из минутного бара
                                        _synTicksCount += synTicks.Count();
                                        _ticks.AddRange(synTicks);
                                    }
                                    _synDays++;
                                }
                            }
                        }
                        date = date.AddDays(1);
                    }
                }

                if (_ticks.Any() && _ticks.Last().InsID == instrum.InsID) // тики внутри каждого инструмента отсортированы по времени, поэтому можно брать последний в списке и он будет последний по времени
                {
                    if (!_insID_lastTick.ContainsKey(instrum.InsID))
                    {
                        _insID_lastTick.Add(instrum.InsID, _ticks.Last());
                    }
                    else // перестраховка
                    {
                        _insID_lastTick[instrum.InsID] = _ticks.Last();
                    }
                }
            }

            await Task.Run(() =>
            {
                lock (_ticks)
                {
                    _ticks = _ticks.OrderBy(t => t.Time).ToList();
                    int count = _ticks.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var tick = _ticks[i];
                        tick.TradeNo = i + 1;
                    }
                }
            });

            return GetTicksCount();
        }

        private IEnumerable<Tick> SynTicks(Bar bar, Instrum instrum)
        {
            decimal p1 = 0, p2 = 0, p3 = 0, q = 0;
            int n1 = 0; int n2 = 0; int n3 = 0;
            List<Tick> ticks = new List<Tick>();

            DateTime time = bar.Time;
            DateTime lastTime = bar.NextBarTime.AddSeconds(-1);
            decimal price = bar.Open;
            long ls = (long)(bar.Volume / instrum.LotSize);
            int lots = (ls > int.MaxValue) ? int.MaxValue : (ls < int.MinValue) ? int.MinValue : (int)ls;
            int lot_step = lots / 60;

            if (bar.Open <= bar.Close) // while bar
            {
                p1 = bar.Open - bar.Low;
                p2 = bar.High - bar.Low;
                p3 = bar.High - bar.Close;
                q = (p1 + p2 + p3) / 60m;
                if (q != 0)
                {
                    n1 = (int)(p1 / q);
                    n3 = (int)(p3 / q);
                }
                if (n1 <= 0) n1 = 1;
                if (n3 <= 0) n3 = 1;

                for (int i = 0; i < n1; i++)
                {
                    decimal pr = Math.Round(price, instrum.Decimals);
                    if (pr > bar.High) pr = bar.High;
                    if (pr < bar.Low) pr = bar.Low;
                    ticks.Add(new Tick(0, time, instrum.InsID, lot_step, pr));
                    price -= q;
                    lots -= lot_step;
                    if (time < lastTime) time = time.AddSeconds(1);
                }

                n2 = 60 - n1 - n3 - 1;
                price = bar.Low;
                for (int i = 0; i < n2; i++)
                {
                    decimal pr = Math.Round(price, instrum.Decimals);
                    if (pr > bar.High) pr = bar.High;
                    if (pr < bar.Low) pr = bar.Low;
                    ticks.Add(new Tick(0, time, instrum.InsID, lot_step, pr));
                    price += q;
                    lots -= lot_step;
                    if (time < lastTime) time = time.AddSeconds(1);
                }

                price = bar.High;
                for (int i = 0; i < n3; i++)
                {
                    decimal pr = Math.Round(price, instrum.Decimals);
                    if (pr > bar.High) pr = bar.High;
                    if (pr < bar.Low) pr = bar.Low;
                    ticks.Add(new Tick(0, time, instrum.InsID, lot_step, pr));
                    price -= q;
                    lots -= lot_step;
                    if (time < lastTime) time = time.AddSeconds(1);
                }
            }
            else // black bar
            {
                p1 = bar.High - bar.Open;
                p2 = bar.High - bar.Low;
                p3 = bar.Close - bar.Low;
                q = (p1 + p2 + p3) / 60m;
                if (q != 0)
                {
                    n1 = (int)(p1 / q);
                    n3 = (int)(p3 / q);
                }
                if (n1 <= 0) n1 = 1;
                if (n3 <= 0) n3 = 1;

                for (int i = 0; i < n1; i++)
                {
                    decimal pr = Math.Round(price, instrum.Decimals);
                    if (pr > bar.High) pr = bar.High;
                    if (pr < bar.Low) pr = bar.Low;
                    ticks.Add(new Tick(0, time, instrum.InsID, lot_step, pr));
                    price += q;
                    lots -= lot_step;
                    if (time < lastTime) time = time.AddSeconds(1);
                }

                n2 = 60 - n1 - n3 - 1;
                price = bar.High;
                for (int i = 0; i < n2; i++)
                {
                    decimal pr = Math.Round(price, instrum.Decimals);
                    if (pr > bar.High) pr = bar.High;
                    if (pr < bar.Low) pr = bar.Low;
                    ticks.Add(new Tick(0, time, instrum.InsID, lot_step, pr));
                    price -= q;
                    lots -= lot_step;
                    if (time < lastTime) time = time.AddSeconds(1);
                }

                price = bar.Low;
                for (int i = 0; i < n3; i++)
                {
                    decimal pr = Math.Round(price, instrum.Decimals);
                    if (pr > bar.High) pr = bar.High;
                    if (pr < bar.Low) pr = bar.Low;
                    ticks.Add(new Tick(0, time, instrum.InsID, lot_step, pr));
                    price += q;
                    lots -= lot_step;
                    if (time < lastTime) time = time.AddSeconds(1);
                }
            }
            ticks.Add(new Tick(0, time, instrum.InsID, lots > 0 ? lots : 0, bar.Close));

            return ticks;
        }

        private int GetTicksCount()
        {
            lock(_ticks)
            {
                return _ticks.Count;
            }
        }

        /// <summary>
        /// Получить статистику по загруженным и синтезированным тикам и датам.
        /// </summary>
        /// <returns>Статистика</returns>
        public TickSourceStatistics GetStatistics()
        {
            return new TickSourceStatistics()
            {
                TotalTicksCount = GetTicksCount(),
                SynTicksCount = _synTicksCount,
                TotalDaysCount = _realDays + _synDays,
                SynDaysCount = _synDays
            };
        }

        #region Run thread
        public delegate void OnStateChangeEH(TestTickSourceState state, int countTicks, int totalTicks);
        public event OnStateChangeEH OnStateChange;

        private bool _isRunning = false;
        private bool _isAbort = false;
        private int _index = 0;

        public bool Start()
        {
            if (_isRunning) return false;

            _isRunning = true;
            _isAbort = false;
            _index = 0;
            Thread job = new Thread(new ThreadStart(DoRun));
            job.Start();

            return true;
        }

        public void Stop()
        {
            _isAbort = true;
        }

        private void DoRun()
        {
            lock (_ticks)
            {
                int countByPercent = _ticks.Count / 100;
                int total = _ticks.Count;
                RaiseStateChange(TestTickSourceState.Running, _index, total);

                while (_index < total)
                {
                    if (_isAbort) break;

                    Tick tick = _ticks[_index];
                    RaiseOnTickEvent(tick);
                    lock (_curTimeLock)
                    {
                        _curTime = tick.Time;
                    }

                    if (countByPercent == 0 || _index % countByPercent == 0)
                        RaiseStateChange(TestTickSourceState.Running, _index, total);

                    _index++;
                };

                if (_index >= total)
                    RaiseStateChange(TestTickSourceState.Completed, _index, total);
                else
                    RaiseStateChange(TestTickSourceState.Stopped, _index, total);
            }
            _isRunning = false;
        }

        private void RaiseStateChange(TestTickSourceState state, int count, int total)
        {
            if (OnStateChange != null)
            {
                if (_syncContext != null)
                    _syncContext.RunAsync(() => { OnStateChange(state, count, total); } );
                else
                    OnStateChange(state, count, total);
            }
        }
        #endregion

        #region ITickSource, ITimeProvider
        public event OnTickEH OnTick;
        private void RaiseOnTickEvent(Tick tick)
        {
            if (OnTick != null)
            {
                if (_syncContext != null)
                    _syncContext.RunAsync(() => { OnTick(tick); });
                else
                    OnTick(tick);
            }
        }

        public DateTime? CurrentTime
        {
            get
            {
                lock (_curTimeLock)
                {
                    if (_curTime != null) return _curTime;
                    if (StartDate != DateTime.MinValue) return StartDate;
                    return null;
                }
            }
        }
        #endregion
    }

    public enum TestTickSourceState
    {
        New = 0,
        Running = 1,
        Stopped = 2,
        Completed = 3
    }

    /// <summary>
    /// Статистика по источнику данных
    /// </summary>
    public class TickSourceStatistics
    {
        /// <summary>
        /// Всего тиков загруженных или синтезированных, по всем инструментам суммарно
        /// </summary>
        public int TotalTicksCount { get; set; }

        /// <summary>
        /// Синтезировано тиков из минутных баров, по всем инструментам суммарно
        /// </summary>
        public int SynTicksCount { get; set; }

        /// <summary>
        /// Всего дней, в которые есть данные загруженные или синтезированные, по всем инструментам суммарно
        /// </summary>
        public int TotalDaysCount { get; set; }

        /// <summary>
        /// Кол-во дней, в которые тиковые данные были синтезированы из минуток, по всем инструментам суммарно
        /// </summary>
        public int SynDaysCount { get; set; }
    }
}
