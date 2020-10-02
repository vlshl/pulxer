using Pulxer.Drawing;
using Common;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Pulxer.Indicators
{
    public interface IIndicator
    {
        IEnumerable<IVisual> GetVisuals();
        XDocument Serialize();
        void Initialize(XDocument xDoc);
        IEnumerable<ValueRowSource> GetSources();
    }
}
