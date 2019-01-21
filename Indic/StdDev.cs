using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indic
{
    public class StdDev : ValueRow
    {
        private ValueRow _source;
        private int _n;
        private ValueRow _mo;

        public StdDev()
            : base()
        {
            _n = 1;
            _mo = new ValueRow();
        }

        public StdDev(ValueRow source, int n)
            : this()
        {
            _source = source;
            _n = n < 1 ? 1 : n;
            if (_source != null)
            {
                _source.Change += Source_Change;
                Source_Change(true);
            }
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

                if (_source != null) _source.Change -= Source_Change; // отписываемся от прежнего, если он был
                _source = value;
                if (_source != null) _source.Change += Source_Change;
                Source_Change(true);
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
                Source_Change(true);
            }
        }

        void Source_Change(bool isReset)
        {
            SuspendEvents();
            if (isReset)
            {
                Clear();
                _mo.Clear();
            }

            if (_source != null)
            {
                int startIndex = isReset ? 0 : this.Count;
                for (int i = startIndex; i <= _source.LastIndex; i++)
                {
                    Calc(i);
                }
            }
            ResumeEvents();
        }

        void Calc(int index)
        {
            if (_mo[index - 1] == null)
            {
                decimal? sum = 0;
                for (int i = index - _n + 1; i <= index; i++)
                {
                    if (_source[i] != null)
                    {
                        sum += _source[i].Value;
                    }
                    else
                    {
                        sum = null; break;
                    }
                }
                _mo.Add(sum / _n);
            }
            else
            {
                if (_source[index - _n] != null && _source[index] != null)
                {
                    decimal next = _mo[index - 1].Value + _source[index].Value / _n - _source[index - _n].Value / _n;
                    _mo.Add(next);
                }
            }

            decimal? s = null;
            if (_mo[index] != null)
            {
                s = 0;
                for (int i = index - _n + 1; i <= index; i++)
                {
                    if (_source[i] == null)
                    {
                        s = null; break;
                    }

                    s += (_source[i] - _mo[index]) * (_source[i] - _mo[index]);
                }
            }

            double? res = null;
            if (s != null)
            {
                res = Math.Sqrt(Convert.ToDouble(s.Value / _n));
            }

            if (res != null)
                this.Add(Convert.ToDecimal(res));
            else
                this.Add(null);
        }
    }
}
