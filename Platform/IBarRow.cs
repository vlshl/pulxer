using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public delegate void OnBarEventHandler(int index);

    public interface IBarRow
    {
        event OnBarEventHandler OnCloseBar;

        Bar this[int i] { get; }
        ValueRow Open { get; }
        ValueRow Close { get; }
        ValueRow High { get; }
        ValueRow Low { get; }
    }
}
