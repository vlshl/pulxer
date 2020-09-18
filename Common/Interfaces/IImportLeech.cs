using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IImportLeech
    {
        Task SyncAccountDataAsync(ISyncPipeServer sps);
        Task FastSyncAccountDataAsync(ISyncPipeServer sps, int lAccountId);
        Task SyncAllTradesAsync();
    }
}
