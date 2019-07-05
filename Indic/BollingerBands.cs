using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indic
{
    public class BollingerBands
    {
        private ValueRow _source;
        private int _n;
        private decimal _width;

        private ValueRow _bbTop;
        private ValueRow _bbBottom;
        private Ma _bbMiddle;
        private StdDev _stdDev;

        public BollingerBands()
        {
            _n = 1;
            _width = 2;
            _bbMiddle = new Ma();
            _stdDev = new StdDev();
            _bbTop = new ValueRow();
            _bbBottom = new ValueRow();
        }

        public BollingerBands(ValueRow source, int n, decimal width)
        {
            _source = source;
            _n = n;
            _width = width >= 0 ? width : 0;

            _bbMiddle = new Ma(source, AverageMethod.Simple, n);
            _stdDev = new StdDev(source, n);
            _bbTop = new ValueRow();
            _bbBottom = new ValueRow();
            if (source != null)
            {
                source.Change += Source_Change;
                Source_Change(null, true);
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
                _bbMiddle.Source = _source;
                _stdDev.Source = _source;
                if (_source != null) _source.Change += Source_Change;
                Source_Change(null, true);
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
                _bbMiddle.N = _n;
                _stdDev.N = _n;
                Source_Change(null, true);
            }
        }

        public decimal Width
        {
            get
            {
                return _width;
            }
            set
            {
                if (value < 0 || _n == value) return;

                _width = value;
                Source_Change(null, true);
            }
        }


        public ValueRow BbTop
        {
            get
            {
                return this._bbTop;
            }
        }

        public ValueRow BbBottom
        {
            get
            {
                return this._bbBottom;
            }
        }

        public ValueRow BbMiddle
        {
            get
            {
                return this._bbMiddle;
            }
        }

        public ValueRow StdDev
        {
            get
            {
                return this._stdDev;
            }
        }

        void Source_Change(ValueRow src, bool isReset)
        {
            _bbTop.SuspendEvents();
            _bbBottom.SuspendEvents();
            if (isReset)
            {
                _bbTop.Clear();
                _bbBottom.Clear();
            }

            if (_source != null)
            {
                int startIndex = isReset ? 0 : _bbTop.Count;
                for (int i = startIndex; i <= _source.LastIndex; i++)
                {
                    CalcTop(i);
                }
                startIndex = isReset ? 0 : _bbBottom.Count;
                for (int i = startIndex; i <= _source.LastIndex; i++)
                {
                    CalcBottom(i);
                }
            }

            _bbTop.ResumeEvents();
            _bbBottom.ResumeEvents();
        }

        private void CalcTop(int index)
        {
            if (this._bbMiddle[index] == null || _stdDev[index] == null)
            {
                this._bbTop.Add(null);
            }
            else
            {
                this._bbTop.Add(this._bbMiddle[index].Value + _stdDev[index] * _width);
            }
        }

        private void CalcBottom(int index)
        {
            if (this._bbMiddle[index] == null || _stdDev[index] == null)
            {
                this._bbBottom.Add(null);
            }
            else
            {
                this._bbBottom.Add(this._bbMiddle[index].Value - _stdDev[index] * _width);
            }
        }
    }
}
