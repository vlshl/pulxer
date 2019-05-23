using Platform;
using System;

namespace Indic
{
    public class Ama : ValueRow
    {
        private ValueRow _source;
        private int _n;
        private decimal _fastSmooth;
        private decimal _slowSmooth;
        private ValueRow _smoothRow = null;

        public Ama()
        {
            _source = null;
            _n = 10;
            _fastSmooth = 2m / 3;
            _slowSmooth = 2m / 31;
        }

        public Ama(ValueRow source, int n, int fastPeriod = 2, int slowPeriod = 30)
            : base()
        {
            _source = source;
            _n = n;

            _fastSmooth = 2m / (fastPeriod + 1);
            _slowSmooth = 2m / (slowPeriod + 1);
            source.Change += Sources_Change;
            if (source.Count > 0) Sources_Change(false);
        }

        /// <summary>
        /// Расчет адаптивной средней по smoothRow.
        /// Значения ряда сглаживания должны лежать в диапазоне [0 .. 1].
        /// При нулевом значении новое значение адаптивной средней совпадает со своим предыдущим значением.
        /// При единичном значении новон значение адаптивной средней совпадает со значением source.
        /// При значении smoothRow = 0,5 новое значение адаптивной средней будет, соответственно, посередине.
        /// </summary>
        /// <param name="source">Источник</param>
        /// <param name="smoothRow">Ряд сглаживания</param>
        public Ama(ValueRow source, ValueRow smoothRow)
            : base()
        {
            _source = source;
            _source.Change += Sources_Change;
            _smoothRow = smoothRow;
            _smoothRow.Change += Sources_Change;
            Sources_Change(false);
        }

        public ValueRow Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (_source == value) return;

                if (_source != null) _source.Change -= Sources_Change;
                _source = value;
                if (_source != null) _source.Change += Sources_Change;
                Sources_Change(true);
            }
        }

        public int N
        {
            get
            {
                return _n;
            }
            set
            {
                if (value < 1 || _n == value) return;

                _n = value;
                Sources_Change(true);
            }
        }

        void Sources_Change(bool isReset)
        {
            if (_smoothRow == null)
            {
                for (int i = this.Count; i <= this._source.LastIndex; i++) Calc(i);
            }
            else
            {
                int min = Math.Min(_source.LastIndex, _smoothRow.LastIndex);
                for (int i = this.Count; i <= min; i++) Calc(i);
            }
        }

        private void Calc(int idx)
        {
            if (this[idx - 1] == null) // this[-1] = null
            {
                this.Add(_source[idx]); return;
            }

            if (_smoothRow == null)
            {
                if (_source[idx] == null || _source[idx - _n] == null)
                {
                    this.Add(null); return;
                }

                decimal direct = Math.Abs(_source[idx].Value - _source[idx - _n].Value);
                decimal volat = 0;
                for (int i = 0; i < _n; i++)
                {
                    if (_source[idx - i] == null || _source[idx - i - 1] == null)
                    {
                        this.Add(null); return;
                    }
                    volat += Math.Abs(_source[idx - i].Value - _source[idx - i - 1].Value);
                }

                decimal er;
                if (volat != 0)
                    er = direct / volat; // [0 .. 1] (0-боковик, 1-тренд)
                else
                    er = direct > 0 ? 1 : 0;

                decimal ssc = er * (_fastSmooth - _slowSmooth) + _slowSmooth;
                decimal? ama = this[idx - 1].Value + ssc * ssc * (_source[idx] - this[idx - 1].Value); // если source[idx]=null, то получаем null
                this.Add(ama);
            }
            else
            {
                if (_smoothRow[idx] == null)
                {
                    this.Add(null); return;
                }

                decimal? ama = _smoothRow[idx] * _source[idx] + (1.0m - _smoothRow[idx]) * this[idx - 1];
                this.Add(ama);
            }
        }
    }
}
