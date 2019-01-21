using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public delegate void OnTickEH(Tick tick);

    /// <summary>
    /// Use for all tick sources
    /// </summary>
    public interface ITickSource
    {
        /// <summary>
        /// On every tick event
        /// </summary>
        event OnTickEH OnTick;
    }
}
