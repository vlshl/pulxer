using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public interface IBotParams
    {
        BotParam GetBotParam(string key);
        int GetIntValue(string key);
        decimal GetDecValue(string key);
        string GetValue(string key);
    }
}
