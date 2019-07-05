using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indic
{
    public enum AverageMethod { Simple, Exponencial, Wilder };

    public class Ma : ValueRow
    {
        private ValueRow _source;
        private AverageMethod _am;
        private int _n = 1;

        /*
         *  сделать базовый класс для всех индикаторов
         *  и вынести туда инициализацию при подписании на источник (если в источнике уже есть данные)
         *  сделать общий метод Calc(i) и переопределять его в конкретных рализациях
         */

        public Ma()
        {
        }

        public Ma(ValueRow source, AverageMethod method, int n)
        {
            _source = source;
            _am = method;
            _n = n < 1 ? 1 : n;

            if (_source != null)
            {
                _source.Change += Sources_Change;
                Sources_Change(null, true);
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

                if (_source != null) _source.Change -= Sources_Change; // отписываемся от прежнего, если он был
                _source = value;
                if (_source != null) _source.Change += Sources_Change;
                Sources_Change(null, true);
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
                Sources_Change(null, true);
            }
        }

        public AverageMethod Method
        {
            get
            {
                return _am;
            }
            set
            {
                if (_am == value) return;

                _am = value;
                Sources_Change(null, true);
            }
        }

        void Sources_Change(ValueRow src, bool isReset)
        {
            SuspendEvents();
            if (isReset) Clear();

            if (_source != null)
            {
                int startIndex = isReset ? 0 : this.Count;
                for (int i = startIndex; i <= _source.LastIndex; i++)
                {
                    if (_am == AverageMethod.Simple)
                        CalcSimple(i);
                    else if (_am == AverageMethod.Exponencial)
                        CalcExponencial(i);
                    else if (_am == AverageMethod.Wilder)
                        CalcWilder(i);
                }
            }
            ResumeEvents();
        }

        private void CalcSimple(int index)
        {
            if (index < _n - 1)
            {
                Add(null);
            }
            else
            {
                if (this[index - 1] == null)
                {
                    decimal? sum = 0;
                    for (int i = index; i >= index - _n + 1; i--)
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

                    if (sum != null) Add(sum.Value / _n); else Add(null);
                }
                else
                {
                    if (_source[index - _n] == null || _source[index] == null)
                    {
                        Add(null);
                    }
                    else
                    {
                        Add(this[index - 1].Value + (_source[index].Value - _source[index - _n].Value) / _n);
                    }
                }
            }
        }

        private void CalcExponencial(int index)
        {
            if (index == 0)
            {
                Add(_source[0]); return;
            }

            if (this[index - 1] == null)
            {
                Add(_source[index]); return;
            }

            if (_source[index] == null)
            {
                Add(null); return;
            }

            decimal? ema = (this[index - 1] * (_n - 1) + 2 * _source[index]) / (_n + 1);
            Add(ema);
        }

        private void CalcWilder(int index)
        {
            if (index == 0)
            {
                Add(_source[0]); return;
            }

            if (this[index - 1] == null)
            {
                Add(_source[index]); return;
            }

            if (_source[index] == null)
            {
                Add(null); return;
            }

            decimal? ema = (this[index - 1] * (_n - 1) + _source[index]) / _n;
            Add(ema);
        }
    }
}
