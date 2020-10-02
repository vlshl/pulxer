using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulxer.Drawing
{
    /// <summary>
    /// Event symbols on chart
    /// </summary>
    public class EventChart : VisualBase
    {
        private EventRow _row;
        private EventSymbol[] _symbols;
        private ChartBrush[] _brushes;

        public EventChart(EventRow row, EventSymbol[] symbols, ChartBrush[] brushes)
        {
            _row = row;
            _symbols = symbols;
            _brushes = brushes;
        }
    }
}
