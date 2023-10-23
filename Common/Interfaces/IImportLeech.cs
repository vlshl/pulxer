using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Common.Interfaces
{
    public interface IImportLeech
    {
        Task FullSyncAccountDataAsync(ISyncPipeServer sps);
        Task SyncAccountDataAsync(ISyncPipeServer sps, int lAccountId);
    }
}
