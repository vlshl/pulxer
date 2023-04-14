using Platform;
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
        string GetPluginsPath();
        int GetHistoryDownloaderDays(string tf);
        int GetHistoryDownloaderMonths(string tf);
        int GetHistoryDownloaderDelay(string tf);
    }
}
