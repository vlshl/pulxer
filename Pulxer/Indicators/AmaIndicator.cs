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
    /// Moving average indicator
    /// </summary>
    public class AmaIndicator : IndicatorBase
    {
        private Ama _ama = null;
        private CurveChart _curveChart = null;
        private AmaSettings _settings;
        private string _name = "";
        private IValueRowSourcesProvider _srcProv = null;
        private AmaNamed _amaNamed = null;

        /// <summary>
        /// AMA indicator constructor
        /// </summary>
        /// <param name="srcProv">ValueRowSource provider</param>
        /// <param name="name">Name</param>
        public AmaIndicator(IValueRowSourcesProvider srcProv, string name, IDependencyManager depManager) : base()
        {
            _srcProv = srcProv;
            _name = name;
            _ama = new Ama();
            _amaNamed = new AmaNamed(_ama);
            _curveChart = new CurveChart(_ama, new ChartBrush(0, 0, 0));
            _settings = new AmaSettings(_ama, _curveChart, srcProv, this, depManager);
        }

        /// <summary>
        /// Get Settings object
        /// </summary>
        /// <returns></returns>
        public override object GetSettings()
        {
            return _settings;
        }

        /// <summary>
        /// Get own source list
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<ValueRowSource> GetSources()
        {
            return new List<ValueRowSource>() { new ValueRowSource(guid, _amaNamed, _ama, this) };
        }

        /// <summary>
        /// Get visual objects to draw
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<IVisual> GetVisuals()
        {
            return new List<IVisual> { _curveChart };
        }

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
                new XElement("Ama",
                    new XAttribute("Id", guid),
                    new XAttribute("Name", _name),
                    new XAttribute("N", _settings.N.ToString()),
                    new XAttribute("SourceId", _settings.Source != null ? _settings.Source.Guid : "")));
        }

        /// <summary>
        /// Initialize from xml
        /// </summary>
        /// <param name="xDoc"></param>
        public override void Initialize(XDocument xDoc)
        {
            var xa_id = xDoc.Root.Attribute("Id");
            if (xa_id != null)
            {
                guid = xa_id.Value;
            }

            var xa_name = xDoc.Root.Attribute("Name");
            if (xa_name != null)
            {
                _name = xa_name.Value;
            }

            var xa_n = xDoc.Root.Attribute("N");
            if (xa_n != null)
            {
                int n = 1;
                if (Int32.TryParse(xa_n.Value, out n)) _settings.N = n;
            }

            var xa_sourceId = xDoc.Root.Attribute("SourceId");
            if (xa_sourceId != null && xa_sourceId.Value.Length > 0)
            {
                var foundSource = _srcProv.GerSourceByID(xa_sourceId.Value);
                if (foundSource != null) _settings.Source = foundSource;
            }
        }
    }

    /// <summary>
    /// AMA named object
    /// </summary>
    public class AmaNamed : INamed
    {
        private Ama _ama = null;

        public AmaNamed(Ama ama)
        {
            if (ama == null) throw new ArgumentNullException();
            _ama = ama;
        }

        public string GetName()
        {
            return string.Format("Ama ({0})", _ama.N.ToString());
        }
    }
}
