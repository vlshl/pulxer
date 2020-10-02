using Indic;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer.Indicators
{
    public class AverageMethodItem
    {
        private AverageMethod _method;

        public AverageMethodItem(AverageMethod m)
        {
            _method = m;
        }

        public AverageMethod Method
        {
            get
            {
                return _method;

            }
        }

        public string Name
        {
            get
            {
                if (_method == AverageMethod.Simple)
                    return "Simple";
                else if (_method == AverageMethod.Exponencial)
                    return "Exponencial";
                else if (_method == AverageMethod.Wilder)
                    return "Welles Wilder's";
                else
                    return "";
            }
        }

        private static List<AverageMethodItem> _items = null;

        public static IEnumerable<AverageMethodItem> AllItems
        {
            get
            {
                if (_items == null)
                {
                    _items = new List<AverageMethodItem>()
                    {
                        new AverageMethodItem(AverageMethod.Simple),
                        new AverageMethodItem(AverageMethod.Exponencial),
                        new AverageMethodItem(AverageMethod.Wilder)
                    };
                }

                return _items;
            }
        }
    }
}
