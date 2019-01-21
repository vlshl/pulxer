using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Indic
{
    public class Macd : Calc2
    {
        private Ma _signal;
        private Calc2 _hist;

        public Macd()
        {
        }

        public Macd(ValueRow src, int longPeriod = 26, int shortPeriod = 12, int signalPeriod = 9, AverageMethod method = AverageMethod.Exponencial)
        {
            this.Source1 = new Ma(src, method, longPeriod);
            this.Source2 = new Ma(src, method, shortPeriod);
            this.CalcMethod = (s1, s2, i) => { return s2[i] - s1[i]; };

            _signal = new Ma(this, method, signalPeriod);
            _hist = new Calc2(this, _signal, (m, s, i) => { return m[i] - s[i]; });
        }

        public ValueRow Signal
        {
            get
            {
                return _signal;
            }
        }

        public ValueRow Histogram
        {
            get
            {
                return _hist;
            }
        }
    }
}
