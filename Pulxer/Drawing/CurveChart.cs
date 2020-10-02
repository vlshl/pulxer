using Platform;
using System;

namespace Pulxer.Drawing
{
    /// <summary>
    /// Curve line
    /// </summary>
    public class CurveChart : VisualBase
    {
        private ValueRow _source;
        private ChartBrush _brush;
        private float _stroke = 1.0f;

        public CurveChart(ValueRow source, ChartBrush brush)
        {
            if (source == null) throw new ArgumentNullException("source");

            this._source = source;
            _source.Change += _source_Change;
            this._brush = brush != null ? brush : new ChartBrush(0, 0, 0);
        }

        void _source_Change(ValueRow vr, bool isReset)
        {
            RaiseChanged();
        }

        public override ChartBrush GetBrush()
        {
            return _brush;
        }

        public ChartBrush Brush
        {
            get
            {
                return _brush;
            }
            set
            {
                if (_brush == value) return;
                _brush = value;
                RaiseChanged();
            }
        }

        public float StrokeWidth
        {
            get
            {
                return _stroke;
            }
            set
            {
                if (_stroke == value) return;
                this._stroke = value;
                RaiseChanged();
            }
        }
    }
}
