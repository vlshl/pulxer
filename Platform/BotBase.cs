using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public interface IBot
    {
        Task<IBotResult> Initialize(IBotParams botParams);
        void Close();
    }

    public abstract class BotBase : IBot
    {
        public virtual Task<IBotResult> Initialize(IBotParams botParams)
        {
            return Task.Factory.StartNew<IBotResult>(() => { return null; });
        }

        public virtual void Close() { }
    }
}
