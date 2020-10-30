using Common;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private bool _isDynamic = false;
        private Dictionary<int, IVisual> _leftVisuals = new Dictionary<int, IVisual>();
        private Dictionary<int, IVisual> _rightVisuals = new Dictionary<int, IVisual>();
        private int _visualKey;

        public ChartData(Timeline tl, int digits, bool isDynamic)
        {
            _timeline = tl;
            _digits = digits;
            _isDynamic = isDynamic;
            _visualKey = 0;
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
        /// Динамический график или статический
        /// </summary>
        public bool IsDynamic
        {
            get
            {
                return _isDynamic;
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
            if (isLeftAxis) _leftVisuals.Add(_visualKey++, vis); else _rightVisuals.Add(_visualKey++, vis);
        }

        /// <summary>
        /// Add visual object to chart
        /// </summary>
        /// <param name="vis">Visual</param>
        /// <param name="isLeftAxis">Left or right Y-axis</param>
        public void AddVisual(IVisual vis, bool isLeftAxis = false)
        {
            vis.Changed += vis_Changed;
            if (isLeftAxis)
            {
                if (!_leftVisuals.ContainsValue(vis)) _leftVisuals.Add(_visualKey++, vis);
            }
            else
            {
                if (!_rightVisuals.ContainsValue(vis)) _rightVisuals.Add(_visualKey++, vis);
            }
        }

        /// <summary>
        /// Remove visual object from chart
        /// </summary>
        /// <param name="vis"></param>
        public void RemoveVisual(IVisual vis)
        {
            vis.Changed -= vis_Changed;

            if (_leftVisuals.ContainsValue(vis))
            {
                var pair = _leftVisuals.FirstOrDefault(r => r.Value == vis);
                _leftVisuals.Remove(pair.Key);
            }

            if (_rightVisuals.ContainsValue(vis))
            {
                var pair = _rightVisuals.FirstOrDefault(r => r.Value == vis);
                _rightVisuals.Remove(pair.Key);
            }

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
                return _leftVisuals.Values;
            }
        }

        /// <summary>
        /// Visual objects of right y-axis
        /// </summary>
        public IEnumerable<IVisual> RightVisuals
        {
            get
            {
                return _rightVisuals.Values;
            }
        }

        /// <summary>
        /// Get all visuals
        /// </summary>
        /// <param name="isLeftAxis">true - left visuals, false (def) - right visuals</param>
        /// <returns></returns>
        public KeyValuePair<int, IVisual>[] GetVisuals(bool isLeftAxis = false)
        {
            if (isLeftAxis)
            {
                return _leftVisuals.ToArray();
            }
            else
            {
                return _rightVisuals.ToArray();
            }
        }

        /// <summary>
        /// Get visual
        /// </summary>
        /// <param name="key">Key of visual</param>
        /// <returns></returns>
        public IVisual GetVisual(int key)
        {
            if (_rightVisuals.ContainsKey(key)) return _rightVisuals[key];
            if (_leftVisuals.ContainsKey(key)) return _leftVisuals[key];
            return null;
        }

        //public IVisual GetLeftVisual(int key)
        //{
        //    if (!_leftVisuals.ContainsKey(key)) return null;
        //    return _leftVisuals[key];
        //}

        //public IVisual GetRigthVisual(int key)
        //{
        //    if (!_leftVisuals.ContainsKey(key)) return null;
        //    return _leftVisuals[key];
        //}
    }

    /// <summary>
    /// Incapsulates RGBA
    /// </summary>
    public class ChartBrush
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte Alpha { get; set; }

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
