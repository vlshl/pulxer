using Pulxer.Drawing;
using Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Pulxer.Indicators
{
    /// <summary>
    /// Base class for all indicators
    /// </summary>
    public abstract class IndicatorBase : IIndicator
    {
        protected string guid = "";

        public IndicatorBase()
        {
            guid = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Indicator Id (IChartIndicator)
        /// </summary>
        public string Id
        {
            get
            {
                return guid;
            }
        }

        /// <summary>
        /// Get all sources (each indicator may give sources for other indicators)
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<ValueRowSource> GetSources()
        {
            return new List<ValueRowSource>();
        }

        /// <summary>
        /// Get all visuals for drawing (IIndicator)
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<IVisual> GetVisuals()
        {
            return new List<IVisual>();
        }

        /// <summary>
        /// Serialize to xml (IIndicator)
        /// </summary>
        /// <returns></returns>
        public virtual XDocument Serialize()
        {
            return new XDocument();
        }

        /// <summary>
        /// Initialize from xml (IIndicator)
        /// </summary>
        /// <param name="xDoc"></param>
        public virtual void Initialize(XDocument xDoc)
        {
        }

        /// <summary>
        /// Indicator settings object (IChartIndicator)
        /// </summary>
        /// <returns></returns>
        public virtual object GetSettings()
        {
            return new object();
        }

        /// <summary>
        /// Indicator name (IChartIndicator)
        /// </summary>
        public virtual string Name
        {
            get { return ""; }
        }
    }
}
