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
        private DateTime? _curTime = null;
        private object _curTimeLock = new object();

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
        /// <returns>Кол-во загруженных тиков</returns>
        public async Task<int> LoadDataAsync()
        {
            lock (_ticks)
            {
                _ticks.Clear();
            }

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
                        }
                        date = date.AddDays(1);
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

            return GetTickCount();
        }

        public int GetTickCount()
        {
            lock(_ticks)
            {
                return _ticks.Count;
            }
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
}
