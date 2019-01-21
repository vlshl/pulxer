using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    /// <summary>
    /// Use for visualization
    /// </summary>
    public class TimeframeItem
    {
        public TimeframeItem(Timeframes tf)
        {
            this.Tf = tf;
            string[] names = TfHelper.GetTimeframeNames(tf);
            this.ShortName = names[0];
            this.Name = names[1];
        }

        public Timeframes Tf { get; private set; }
        public string Name { get; private set; }
        public string ShortName { get; private set; }
    }
}
