using Common;
using Common.Interfaces;
using Indic;
using Pulxer.Drawing;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Pulxer.Indicators
{
    /// <summary>
    /// Bollinger bands indicator
    /// </summary>
    public class BbIndicator : IndicatorBase
    {
        private readonly BollingerBands _bb;
        private readonly CurveChart _curveMiddle;
        private readonly CurveChart _curveTop;
        private readonly CurveChart _curveBottom;
        private readonly BbSettings _settings;
        private readonly string _name;
        private readonly IValueRowSourcesProvider _srcProv;

        /// <summary>
        /// Bollinger bands indicator constructor
        /// </summary>
        /// <param name="srcProv">ValueRowSource provider</param>
        /// <param name="name">Name</param>
        public BbIndicator(IValueRowSourcesProvider srcProv, string name, IDependencyManager depManager) : base()
        {
            _srcProv = srcProv;
            _name = name;
            _bb = new BollingerBands();
            _curveMiddle = new CurveChart(_bb.BbMiddle, new ChartBrush(0, 0, 0));
            _curveTop = new CurveChart(_bb.BbTop, new ChartBrush(0, 0, 0));
            _curveBottom = new CurveChart(_bb.BbBottom, new ChartBrush(0, 0, 0));
            _settings = new BbSettings(_bb, _curveMiddle, _curveTop, _curveBottom, _srcProv, this, depManager);
        }

        /// <summary>
        /// Get settings object
        /// </summary>
        /// <returns></returns>
        public override object GetSettings()
        {
            return _settings;
        }

        /// <summary>
        /// Get all own ValueRow sources (top, middle and bottom are ValueRow sources)
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<ValueRowSource> GetSources()
        {
            return new List<ValueRowSource>() 
            { 
                new ValueRowSource(guid + ":M", new BbNamed(_bb, BollingerBandsCurve.Middle), _bb.BbMiddle, this),
                new ValueRowSource(guid + ":T", new BbNamed(_bb, BollingerBandsCurve.Top), _bb.BbTop, this),
                new ValueRowSource(guid + ":B", new BbNamed(_bb, BollingerBandsCurve.Bottom), _bb.BbBottom, this)
            };
        }

        /// <summary>
        /// Get all visuals for drawing indicator
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IVisual> GetVisuals()
        {
            return new List<IVisual> { _curveMiddle, _curveTop, _curveBottom };
        }

        /// <summary>
        /// Get name
        /// </summary>
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Serialize to xml
        /// </summary>
        /// <returns></returns>
        public override XDocument Serialize()
        {
            return new XDocument(
                new XElement("Bb", 
                    new XAttribute("Id", guid),
                    new XAttribute("N", _settings.N.ToString()),
                    new XAttribute("Width", _settings.Width.ToString()),
                    new XAttribute("SourceId", _settings.Source != null ? _settings.Source.Guid : "")));
        }

        /// <summary>
        /// Initialize from xml
        /// </summary>
        /// <param name="xDoc"></param>
        public override void Initialize(XDocument xDoc)
        {
            var xaId = xDoc.Root.Attribute("Id");
            if (xaId != null)
            {
                guid = xaId.Value;
            }

            var xaN = xDoc.Root.Attribute("N");
            if (xaN != null)
            {
                int n = 1;
                if (int.TryParse(xaN.Value, out n)) _settings.N = n;
            }

            var xaWidth = xDoc.Root.Attribute("Width");
            if (xaWidth != null)
            {
                decimal width = 1;
                if (decimal.TryParse(xaWidth.Value, out width)) _settings.Width = width;
            }

            var xaSourceId = xDoc.Root.Attribute("SourceId");
            if (xaSourceId != null && xaSourceId.Value.Length > 0)
            {
                var foundSource = _srcProv.GerSourceByID(xaSourceId.Value);
                if (foundSource != null) _settings.Source = foundSource;
            }
        }
    }

    /// <summary>
    /// Bollinger Bands Named
    /// </summary>
    public class BbNamed : INamed
    {
        private readonly BollingerBands _bb;
        private readonly BollingerBandsCurve _bbc;

        public BbNamed(BollingerBands bb, BollingerBandsCurve bbc)
        {
            if (bb == null) throw new ArgumentNullException();
            _bb = bb;
            _bbc = bbc;
        }

        public string GetName()
        {
            var bbc = "";
            switch (_bbc)
            {
                case BollingerBandsCurve.Bottom:
                    bbc = "Bot";
                    break;
                case BollingerBandsCurve.Top:
                    bbc = "Top";
                    break;
                case BollingerBandsCurve.Middle:
                    bbc = "Mid";
                    break;
            }
            return string.Format("BB{0} ({1})", bbc, _bb.N.ToString());
        }
    }

    public enum BollingerBandsCurve
    {
        Bottom = 1, Middle = 2, Top = 3
    }
}
