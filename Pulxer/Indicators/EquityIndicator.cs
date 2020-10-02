using Pulxer.Drawing;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Pulxer.Indicators
{
    public class EquityIndicator : IndicatorBase
    {
        private Equity _equity;
        //private CurveChart _cashChart = null;
        //private CurveChart _pfChart = null;
        private CurveChart _eqChart = null;

        public EquityIndicator(Equity equity) : base()
        {
            _equity = equity;
            //_cashChart = new CurveChart(_equity.CashRow, new ChartBrush(0, 200, 0));
            //_pfChart = new CurveChart(_equity.PortfolioRow, new ChartBrush(255, 0, 0));
            _eqChart = new CurveChart(_equity.EquityRow, new ChartBrush(0, 0, 255));
        }

        public override IEnumerable<IVisual> GetVisuals()
        {
            return new List<IVisual> { _eqChart };
        }

        public override string Name
        {
            get
            {
                return "Equity";
            }
        }

        public override XDocument Serialize()
        {
            return new XDocument(
                new XElement("Equity",
                    new XAttribute("Id", guid)));
        }

        public override void Initialize(XDocument xDoc)
        {
            var xa_id = xDoc.Root.Attribute("Id");
            if (xa_id != null)
            {
                guid = xa_id.Value;
            }
        }
    }
}
