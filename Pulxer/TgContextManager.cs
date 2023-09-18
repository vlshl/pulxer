using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Pulxer
{
    public class TgContextManager : ITgContextManager
    {
        private List<ITgContext> _contexts;
        private TgService _tgSvc;
        private ITgContext _curContext;

        public TgContextManager()
        {
            _contexts = new List<ITgContext>();
            _curContext = null;
        }

        public void Init(TgService tgSvc)
        {
            _tgSvc = tgSvc;
        }

        public bool RegisterContext(ITgContext ctx) 
        {
            if (ctx == null) return false;

            IEnumerable<string[]> ctxList;
            lock (_contexts)
            {
                if (_contexts.Contains(ctx)) return false;

                _contexts.Add(ctx);
                ctxList = _contexts.Select(p => new string[] { p.GetTgCommand(), p.GetTgName() });
            }
            _tgSvc?.OnChangeContextList(ctxList);

            return true;
        }

        public bool UnregisterContext(ITgContext ctx)
        {
            if (ctx == null) return false;

            string cmd = ctx.GetTgCommand();
            if (!cmd.StartsWith("/")) cmd = "/" + cmd;

            IEnumerable<string[]> ctxList;
            lock (_contexts)
            {
                if (!_contexts.Contains(ctx)) return false;

                _contexts.Remove(ctx);
                ctxList = _contexts.Select(p => new string[] { cmd, p.GetTgName() });
            }
            _tgSvc?.OnChangeContextList(ctxList);

            return true;
        }

        public async Task SendMessage(ITgContext ctx, string msg, IEnumerable<string[]> buttons = null, IEnumerable<int> countInRows = null)
        {
            await _tgSvc.SendMessage(msg, buttons, countInRows);
        }

        public void OnMessage(string msg)
        {
            string cmd = msg.StartsWith("/") ? msg : "/" + msg;

            ITgContext foundCtx = null;
            lock (_contexts)
            {
                foundCtx = _contexts.FirstOrDefault(c => c.GetTgCommand() == cmd);
            }
            if (foundCtx != null) 
            {
                _curContext = foundCtx;
                _curContext.OnSetTgContext();
            }
            else
            {
                if (_curContext != null) _curContext.OnMessage(msg);
            }
        }

        public void OnButton(string cmd)
        {
            if (_curContext == null)
            {
                _tgSvc.SendMessage("No current context").Wait();
                return;
            }

            _curContext.OnCommand(cmd);
        }
    }
}
