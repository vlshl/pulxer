using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IConfig
    {
        string GetLeechDataPath();
        string GetHistoryProviderConfig();
        string GetHistoryProviderCache();
        string GetBotsPath();
    }
}
