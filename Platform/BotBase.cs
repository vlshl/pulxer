using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public interface IBot
    {
        void Initialize(IBotParams botParams);
        void Close();
    }

    public class BotBase : IBot
    {
        public virtual void Initialize(IBotParams botParams) { }
        public virtual void Close() { }
    }
}
