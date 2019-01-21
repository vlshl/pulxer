using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indic
{
    public delegate decimal? CalcDelegate(ValueRow source, int index);

    public class Calc : ValueRow
    {
        private ValueRow _source;
        private CalcDelegate _calc;

        public Calc(ValueRow source, CalcDelegate calc)
        {
            this.Source = source;
            this.CalcMethod = calc;
        }

        public Calc(ValueRow source)
        {
            this.Source = source;
        }

        public Calc()
        {
        }

        public ValueRow Source
        {
            set
            {
                if (_source == value) return;

                if (_source != null) _source.Change -= Source_Change;
                _source = value;
                if (_source != null)
                {
                    _source.Change += Source_Change;
                    Source_Change(true);
                }
                else
                {
                    Clear();
                }
            }
        }

        public CalcDelegate CalcMethod
        {
            set
            {
                if (_calc == value) return;
                _calc = value;
                if (_calc != null && _source != null)
                {
                    Source_Change(true);
                }
                else
                {
                    Clear();
                }
            }
        }

        void Source_Change(bool isReset)
        {
            if (_calc == null || _source == null) return;

            int startIndex = isReset ? 0 : this.Count;
            for (int i = startIndex; i <= _source.LastIndex; i++)
            {
                this.Add(_calc(_source, i));
            }
        }
    }
}
