using Common;
using Common.Interfaces;
using Indic;
using Pulxer.Drawing;
using System.Collections.Generic;

namespace Pulxer.Indicators
{
    /// <summary>
    /// Bollinger Bands Settings object (ViewModel)
    /// </summary>
    public class BbSettings : INotifyObject
    {
        private BollingerBands _bb = null;
        private CurveChart _curveMiddle = null;
        private CurveChart _curveTop = null;
        private CurveChart _curveBottom = null;
        private ValueRowSource _source = null;
        private IValueRowSourcesProvider _srvProv = null;
        private BbIndicator _bbIndic = null;
        private IDependencyManager _depManager;

        public BbSettings(BollingerBands bb, CurveChart curveMiddle, CurveChart curveTop, CurveChart curveBottom, 
            IValueRowSourcesProvider srvProv, BbIndicator bbIndic, IDependencyManager depManager)
        {
            _bb = bb; 
            _curveMiddle = curveMiddle;
            _curveTop = curveTop;
            _curveBottom = curveBottom;
            _srvProv = srvProv;
            _bbIndic = bbIndic;
            _depManager = depManager;
        }

        /// <summary>
        /// Get all available ValueItemSources for Source property
        /// </summary>
        public IEnumerable<ValueRowSource> SourceItems
        {
            get
            {
                return _srvProv.GetSources(_bbIndic.Id);
            }
        }

        /// <summary>
        /// Source for BB
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
                _depManager.SingleDepend(_bbIndic, _source.Owner);
                _bb.Source = _source.ValueRow;
                RaisePropertyChanged("Source");

            }
        }

        /// <summary>
        /// N parameter
        /// </summary>
        public int N
        {
            get
            {
                return _bb.N;
            }
            set
            {
                _bb.N = value;
                RaisePropertyChanged("N");
            }
        }

        /// <summary>
        /// Width parameter
        /// </summary>
        public decimal Width
        {
            get
            {
                return _bb.Width;
            }
            set
            {
                _bb.Width = value;
                RaisePropertyChanged("Width");
            }
        }
    }
}
