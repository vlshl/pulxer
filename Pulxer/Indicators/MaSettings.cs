using Common;
using Common.Interfaces;
using Indic;
using Pulxer.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace Pulxer.Indicators
{
    /// <summary>
    /// Ma settings object
    /// </summary>
    public class MaSettings : INotifyObject
    {
        private Ma _ma = null;
        private CurveChart _curveChart = null;
        private ValueRowSource _source = null;
        private IValueRowSourcesProvider _srcProv = null;
        private MaIndicator _maIndic = null;
        private IDependencyManager _depManager = null;

        public MaSettings(Ma ma, CurveChart curveChart, IValueRowSourcesProvider srcProv, MaIndicator maIndic,
            IDependencyManager depManager)
        {
            _ma = ma;
            _curveChart = curveChart;
            _srcProv = srcProv;
            _maIndic = maIndic;
            _depManager = depManager;
        }

        /// <summary>
        /// Get all available sources for Source property
        /// </summary>
        public IEnumerable<ValueRowSource> SourceItems
        {
            get
            {
                return _srcProv.GetSources(_maIndic.Id);
            }
        }

        /// <summary>
        /// Source for moving average
        /// </summary>
        public ValueRowSource Source
        {
            get
            {
                return _source;
            }
            set
            {
                if (value == null) return;

                _source = value;
                _ma.Source = _source.ValueRow;
                _depManager.SingleDepend(_maIndic, _source.Owner);
                RaisePropertyChanged("Source");
            }
        }

        /// <summary>
        /// N parameter for moving average
        /// </summary>
        public int N
        {
            get
            {
                return _ma.N;
            }
            set
            {
                _ma.N = value;
                RaisePropertyChanged("N");
            }
        }

        public IEnumerable<AverageMethodItem> AverageMethodItems
        {
            get
            {
                return AverageMethodItem.AllItems;
            }
        }

        public AverageMethodItem AverageMethodItem
        {
            get
            {
                return AverageMethodItem.AllItems.FirstOrDefault(r => r.Method == _ma.Method);
            }
            set
            {
                if (value == null) return;

                _ma.Method = value.Method;
                RaisePropertyChanged("AverageMethodItem");
            }
        }
    }
}
