using Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IConfig
    {
        string GetHistoryProviderConfig();
        string GetHistoryProviderCache();
        string GetBotsPath();
        string GetPluginsPath();
        string GetTickHistoryPath();
        int GetHistoryDownloaderDays(string tf);
        int GetHistoryDownloaderMonths(string tf);
        int GetHistoryDownloaderDelay(string tf);
    }
}
