using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pulxer
{
    public abstract class TgContext : ITgContext
    {
        public TgContext()
        {
        }

        public virtual string GetTgName()
        {
            return "";
        }

        public virtual string GetTgCommand()
        {
            return "";
        }

        public virtual void OnSetTgContext()
        {
        }

        public virtual void OnCommand(string cmd)
        {
        }

        public virtual void OnMessage(string msg)
        {
        }
    }
}
