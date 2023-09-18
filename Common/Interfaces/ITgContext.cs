using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface ITgContext
    {
        string GetTgCommand();
        string GetTgName();
        void OnSetTgContext();
        void OnCommand(string cmd);
        void OnMessage(string msg);
    }
}
