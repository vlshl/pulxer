using Common;
using Common.Interfaces;
using Indic;
using Pulxer.Drawing;
using System.Collections.Generic;

namespace Pulxer.Indicators
{
    /// <summary>
    /// Ma settings object
    /// </summary>
    public class AmaSettings : INotifyObject
    {
        private Ama _ama = null;
        private CurveChart _curveChart = null;
        private ValueRowSource _source = null;
        private IValueRowSourcesProvider _srcProv = null;
        private AmaIndicator _amaIndic = null;
        private IDependencyManager _depManager = null;

        public AmaSettings(Ama ama, CurveChart curveChart, IValueRowSourcesProvider srcProv, AmaIndicator amaIndic,
            IDependencyManager depManager)
        {
            _ama = ama;
            _curveChart = curveChart;
            _srcProv = srcProv;
            _amaIndic = amaIndic;
            _depManager = depManager;
        }

        /// <summary>
        /// Get all available sources for Source property
        /// </summary>
        public IEnumerable<ValueRowSource> SourceItems
        {
            get
            {
                return _srcProv.GetSources(_amaIndic.Id);
            }
        }

        /// <summary>
        /// Source for AMA
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
                _ama.Source = _source.ValueRow;
                _depManager.SingleDepend(_amaIndic, _source.Owner);
                RaisePropertyChanged("Source");
            }
        }

        /// <summary>
        /// N parameter for AMA
        /// </summary>
        public int N
        {
            get
            {
                return _ama.N;
            }
            set
            {
                _ama.N = value;
                RaisePropertyChanged("N");
            }
        }
    }
}
