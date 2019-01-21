using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    /// <summary>
    /// The notifying price bar list
    /// </summary>
    public class BarRow : IBarRow
    {
        /// <summary>
        /// Close bar event raise when AddTick() added the first tick of the next bar.
        /// Index of closed bar.
        /// </summary>
        public event OnBarEventHandler OnCloseBar;

        /// <summary>
        /// Raised when the price bar changes.
        /// The price bars changes from Index to last.
        /// </summary>
        public event OnBarEventHandler OnChangeBar;

        private List<Bar> _bars;
        private Timeline _timeline;
        private ValueRow _openVR;
        private ValueRow _closeVR;
        private ValueRow _highVR;
        private ValueRow _lowVR;
        private ValueRow _medianVR;
        private ValueRow _typicalVR;
        private Bar _curBar;
        private DateTime _nextBarTime;
        private bool _isNewBar = true; // the next AddTick() must begin the next bar
        private ITickSource _tSource = null;
        private ITickDispatcher _tDisp = null;
        private int _insID = 0;
        private bool _isSuspended = false;
        private int? _lastCloseBarIndex = null;
        private int? _lastChangeBarIndex = null;

        public BarRow(Timeframes tf, int insID)
        {
            _bars = new List<Bar>();
            _timeline = new Timeline(tf);
            _openVR = new ValueRow();
            _closeVR = new ValueRow();
            _highVR = new ValueRow();
            _lowVR = new ValueRow();
            _medianVR = new ValueRow();
            _typicalVR = new ValueRow();
            _nextBarTime = DateTime.MinValue;
            _insID = insID;
        }

        public BarRow(Timeframes tf, ITickSource tickSrc, int insID)
            : this(tf, insID)
        {
            _tSource = tickSrc;
            _tSource.OnTick += TSource_Tick;
            _insID = insID;
        }

        public BarRow(Timeframes tf, ITickDispatcher tickDisp, int insID)
            : this(tf, insID)
        {
            _tDisp = tickDisp;
            _insID = insID;
            _tDisp.Subscribe(this, insID, Td_Tick);
        }

        public void CloseBarRow()
        {
            if (_tSource != null) _tSource.OnTick -= TSource_Tick;
            if (_tDisp != null) _tDisp.Unsubscribe(this, _insID);
            _tSource = null;
            _tDisp = null;
        }

        public int InstrumID
        {
            get
            {
                return _insID;
            }
        }

        public Timeframes Timeframe
        {
            get
            {
                return _timeline.Timeframe;
            }
        }

        public Timeline Dates
        {
            get
            {
                return _timeline;
            }
        }

        void TSource_Tick(Tick tick)
        {
            if (tick.InsID == _insID)
                AddTick(tick.Time, tick.Price, tick.Lots);
        }

        void Td_Tick(Tick tick)
        {
            AddTick(tick.Time, tick.Price, tick.Lots);
        }

        /// <summary>
        /// Add new tick to BarRow.
        /// BarRow automatically creates new bars.
        /// </summary>
        /// <param name="time"></param>
        /// <param name="price"></param>
        /// <param name="count"></param>
        public void AddTick(DateTime time, decimal price, long count)
        {
            if (Timeframe == Timeframes.Tick)
            {
                _curBar = new Bar(time, time, price, price, price, price, count);
                CloseBar();
            }
            else
            {
                if (_isNewBar)
                {
                    DateTime[] bds = TfHelper.GetDates(time, Timeframe);
                    _curBar = new Bar(bds[0], bds[1], price, price, price, price, count);
                    _nextBarTime = bds[1];
                    _isNewBar = false;
                    _bars.Add(_curBar);
                    _timeline.Add(_curBar.Time, Timeframe);
                    RaiseOnChangeBar(_bars.Count - 1);
                }
                else
                {
                    if (time < _nextBarTime)
                    {
                        _curBar.Close = price;
                        if (_curBar.High < price) _curBar.High = price;
                        if (_curBar.Low > price) _curBar.Low = price;
                        _curBar.Volume += count;
                        RaiseOnChangeBar(_bars.Count - 1);
                    }
                    else
                    {
                        CloseBar();
                        DateTime[] bds = TfHelper.GetDates(time, Timeframe);
                        _curBar = new Bar(bds[0], bds[1], price, price, price, price, count);
                        _nextBarTime = bds[1];
                        _bars.Add(_curBar);
                        _timeline.Add(_curBar.Time, Timeframe);
                        RaiseOnChangeBar(_bars.Count - 1);
                    }
                }
            }
        }

        private void CloseBar()
        {
            _openVR.Add(_curBar.Open);
            _closeVR.Add(_curBar.Close);
            _highVR.Add(_curBar.High);
            _lowVR.Add(_curBar.Low);
            _medianVR.Add((_curBar.Low + _curBar.High) / 2);
            _typicalVR.Add((_curBar.Low + _curBar.High + _curBar.Close) / 3);

            RaiseOnCloseBar(_bars.Count - 1);
        }

        /// <summary>
        /// Force closing the last price bar
        /// </summary>
        public void CloseLastBar()
        {
            if (_curBar == null) return;

            CloseBar();
            _isNewBar = true;
        }

        /// <summary>
        /// Suspend raise events
        /// </summary>
        public void SuspendEvents()
        {
            _isSuspended = true;
            _openVR.SuspendEvents();
            _closeVR.SuspendEvents();
            _highVR.SuspendEvents();
            _lowVR.SuspendEvents();
            _medianVR.SuspendEvents();
            _typicalVR.SuspendEvents();
            _lastChangeBarIndex = _lastCloseBarIndex = null;
        }

        /// <summary>
        /// Resume raise events and raise pending events
        /// </summary>
        public void ResumeEvents()
        {
            _openVR.ResumeEvents();
            _closeVR.ResumeEvents();
            _highVR.ResumeEvents();
            _lowVR.ResumeEvents();
            _medianVR.ResumeEvents();
            _typicalVR.ResumeEvents();

            _isSuspended = false;
            if (_lastChangeBarIndex != null && _lastCloseBarIndex != null)
            {
                if (_lastChangeBarIndex.Value <= _lastCloseBarIndex.Value)
                {
                    RaiseOnChangeBar(_lastChangeBarIndex.Value);
                    RaiseOnCloseBar(_lastCloseBarIndex.Value);
                }
                else
                {
                    RaiseOnCloseBar(_lastCloseBarIndex.Value);
                    RaiseOnChangeBar(_lastChangeBarIndex.Value);
                }
            }
            else if (_lastChangeBarIndex != null)
            {
                RaiseOnChangeBar(_lastChangeBarIndex.Value);
            }
            else if (_lastCloseBarIndex != null)
            {
                RaiseOnCloseBar(_lastCloseBarIndex.Value);
            }

            _lastChangeBarIndex = _lastCloseBarIndex = null;
        }

        /// <summary>
        /// Clear all data
        /// </summary>
        public void Clear()
        {
            _bars.Clear();
            _timeline.Clear();
            _openVR.Clear();
            _closeVR.Clear();
            _highVR.Clear();
            _lowVR.Clear();
            _medianVR.Clear();
            _typicalVR.Clear();
            _nextBarTime = DateTime.MinValue;
            _isNewBar = true;
            _isSuspended = false;
            _lastCloseBarIndex = null;
            _lastChangeBarIndex = null;
        }

        private void RaiseOnCloseBar(int index)
        {
            if (_isSuspended)
            {
                _lastCloseBarIndex = index;
            }
            else
            {
                OnCloseBar?.Invoke(index);
            }
        }

        private void RaiseOnChangeBar(int index)
        {
            if (_isSuspended)
            {
                _lastChangeBarIndex = index;
            }
            else
            {
                OnChangeBar?.Invoke(index);
            }
        }

        /// <summary>
        /// Price bar by index
        /// </summary>
        /// <param name="i">Index</param>
        /// <returns>Price bar or null</returns>
        public Bar this[int i]
        {
            get
            {
                if (i < 0 || i >= _bars.Count) return null;
                return _bars[i];
            }
        }

        /// <summary>
        /// Current price bars array.
        /// The number of bars can be one more than in the ranks of Open, Close, etc.
        /// The last bar in the Bars list changes with each new tick,
        /// and the series Open, Close, etc. add only after calling CloseLastBar()
        /// </summary>
        public IEnumerable<Bar> Bars
        {
            get
            {
                return _bars.ToList();
            }
        }

        /// <summary>
        /// Open prices of bars
        /// </summary>
        public ValueRow Open
        {
            get
            {
                return _openVR;
            }
        }

        /// <summary>
        /// Close prices of bars
        /// </summary>
        public ValueRow Close
        {
            get
            {
                return _closeVR;
            }
        }

        /// <summary>
        /// High prices of bars
        /// </summary>
        public ValueRow High
        {
            get
            {
                return _highVR;
            }
        }

        /// <summary>
        /// Low prices of bars
        /// </summary>
        public ValueRow Low
        {
            get
            {
                return _lowVR;
            }
        }

        /// <summary>
        /// Median prices of bars: median = (high + low) / 2
        /// </summary>
        public ValueRow Median
        {
            get
            {
                return _medianVR;
            }
        }

        /// <summary>
        /// Typical prices of bars: typical = (high + low + close) / 3
        /// </summary>
        public ValueRow Typical
        {
            get
            {
                return _typicalVR;
            }
        }

        /// <summary>
        /// Length of the bars array
        /// </summary>
        public int Count
        {
            get
            {
                return _bars.Count;
            }
        }
    }
}
