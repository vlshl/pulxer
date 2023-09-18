using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface ITgContextManager
    {
        bool RegisterContext(ITgContext ctx);
        bool UnregisterContext(ITgContext ctx);
        Task SendMessage(ITgContext ctx, string msg, IEnumerable<string[]> buttons = null, IEnumerable<int> countInRows = null);
    }
}
