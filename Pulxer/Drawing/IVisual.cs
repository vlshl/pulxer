using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer.Drawing
{
    /// <summary>
    /// Base interface for all visual objects
    /// </summary>
    public interface IVisual
    {
        event VisualChangedEH Changed; // when visual object changes and needs redraw
        ChartBrush GetBrush(); // current brush
    }
}
