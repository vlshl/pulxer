using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indic
{
    public delegate decimal? CalcDelegate2(ValueRow source1, ValueRow source2, int index);

    public class Calc2 : ValueRow
    {
        private ValueRow _source1;
        private ValueRow _source2;
        private CalcDelegate2 _calc;

        public Calc2(ValueRow source1, ValueRow source2, CalcDelegate2 calc)
        {
            _source1 = source1;
            _source2 = source2;
            _calc = calc;
        }

        public Calc2(ValueRow source1, ValueRow source2)
        {
            _source1 = source1;
            _source2 = source2;
        }

        public Calc2()
        {
        }

        public ValueRow Source1
        {
            set
            {
                if (_source1 == value) return;

                if (_source1 != null) _source1.Change -= Source_Change;
                _source1 = value;
                if (_source1 != null)
                {
                    _source1.Change += Source_Change;
                    Source_Change(true);
                }
                else
                {
                    Clear();
                }
            }
        }

        public ValueRow Source2
        {
            set
            {
                if (_source2 == value) return;

                if (_source2 != null) _source2.Change -= Source_Change;
                _source2 = value;
                if (_source2 != null)
                {
                    _source2.Change += Source_Change;
                    Source_Change(true);
                }
                else
                {
                    Clear();
                }
            }
        }

        public CalcDelegate2 CalcMethod
        {
            set
            {
                if (_calc == value) return;
                _calc = value;
                if (_calc != null && _source1 != null)
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
            if (_calc == null || _source1 == null || _source2 == null) return;

            int startIndex = isReset ? 0 : this.Count;
            int endIndex = Math.Min(_source1.LastIndex, _source2.LastIndex);
            for (int i = startIndex; i <= endIndex; i++)
            {
                this.Add(_calc(_source1, _source2, i));
            }
        }
    }
}
