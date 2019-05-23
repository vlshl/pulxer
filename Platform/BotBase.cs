using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public interface IBot
    {
        Task<bool> Initialize(IBotParams botParams);
        void Close();
    }

    public abstract class BotBase : IBot
    {
        public virtual Task<bool> Initialize(IBotParams botParams)
        {
            return Task.Factory.StartNew<bool>(() => { return true; });
        }

        public virtual void Close() { }
    }
}
