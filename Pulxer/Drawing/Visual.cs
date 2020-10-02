using Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer.Drawing
{
    public delegate void VisualChangedEH(object sender, EventArgs e);

    public abstract class VisualBase : IVisual
    {
        virtual public event VisualChangedEH Changed;

        virtual public ChartBrush GetBrush()
        {
            return null;
        }

        protected void RaiseChanged()
        {
            if (Changed != null) Changed(this, new EventArgs());
        }
    }
}
