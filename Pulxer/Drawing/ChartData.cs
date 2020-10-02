using Common;
using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer.Drawing
{
    public delegate void OnRedrawEH();

    /// <summary>
    /// Contains all chart visual objects
    /// </summary>
    public class ChartData
    {
        /// <summary>
        /// If redraw needs
        /// </summary>
        public event OnRedrawEH OnRedraw;

        private Timeline _timeline = null;
        private int _digits = 0;
        private List<IVisual> _leftVisuals = new List<IVisual>();
        private List<IVisual> _rightVisuals = new List<IVisual>();

        public ChartData(Timeline tl, int digits)
        {
            _timeline = tl;
            _digits = digits;
        }

        /// <summary>
        /// Timeline
        /// </summary>
        public Timeline Timeline
        {
            get
            {
                return _timeline;
            }
        }

        /// <summary>
        /// Decimal places
        /// </summary>
        public int Digits
        {
            get
            {
                return _digits;
            }
        }

        /// <summary>
        /// Add prices object to chart
        /// </summary>
        /// <param name="bars">Price bars</param>
        /// <param name="brush">Brush</param>
        /// <param name="isLeftAxis">Left or right Y-axis</param>
        public void AddPrices(BarRow bars, ChartBrush brush, bool isLeftAxis = false)
        {
            if (bars.Dates != _timeline) throw new Exception("Timeline incorrect.");

            PriceChart vis = new PriceChart(bars, brush);
            vis.Changed += vis_Changed;
            if (isLeftAxis) _leftVisuals.Add(vis); else _rightVisuals.Add(vis);
        }

        /// <summary>
        /// Add visual object to chart
        /// </summary>
        /// <param name="vis">Visual</param>
        /// <param name="isLeftAxis">Left or right Y-axis</param>
        public void AddVisual(IVisual vis, bool isLeftAxis = false)
        {
            vis.Changed += vis_Changed;
            if (isLeftAxis) _leftVisuals.Add(vis); else _rightVisuals.Add(vis);
        }

        /// <summary>
        /// Remove visual object from chart
        /// </summary>
        /// <param name="vis"></param>
        public void RemoveVisual(IVisual vis)
        {
            vis.Changed -= vis_Changed;
            _leftVisuals.Remove(vis);
            _rightVisuals.Remove(vis);
            if (OnRedraw != null) OnRedraw();
        }

        void vis_Changed(object sender, EventArgs e)
        {
            if (OnRedraw != null) OnRedraw();
        }

        /// <summary>
        /// Visual objects of left y-axis
        /// </summary>
        public IEnumerable<IVisual> LeftVisuals
        {
            get
            {
                return _leftVisuals;
            }
        }

        /// <summary>
        /// Visual objects of right y-axis
        /// </summary>
        public IEnumerable<IVisual> RightVisuals
        {
            get
            {
                return _rightVisuals;
            }
        }
    }

    /// <summary>
    /// Incapsulates RGBA
    /// </summary>
    public class ChartBrush
    {
        public byte Red;
        public byte Green;
        public byte Blue;
        public byte Alpha;

        public ChartBrush(byte r, byte g, byte b, byte a = 255)
        {
            this.Red = r; this.Green = g; this.Blue = b; this.Alpha = a;
        }
    }

    /// <summary>
    /// Event symbols for chart events
    /// </summary>
    public enum EventSymbol
    {
        UpTriangle = 0,
        DownTriangle = 1,
        Floor = 2,
        Ceiling = 3
    }
}
