using Pulxer.Drawing;

namespace Pulxer.Indicators
{
    public class MarkerBase
    {
        public ChartMarkers Marker { get; private set; }

        public MarkerBase(ChartMarkers marker)
        {
            Marker = marker;
        }

        /// <summary>
        /// Get visual for drawing
        /// </summary>
        /// <returns></returns>
        public virtual IVisual GetVisual()
        {
            return null;
        }
    }
}
