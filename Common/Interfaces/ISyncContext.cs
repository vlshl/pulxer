using System;

namespace Common.Interfaces
{
    /// <summary>
    /// Интерфейс контекста синхронизации
    /// </summary>
    public interface ISyncContext
    {
        void RunAsync(Action action);
    }
}
