using System;

namespace Common.Interfaces
{
    /// <summary>
    /// Time provider interface
    /// </summary>
    public interface ITimeProvider
    {
        DateTime? CurrentTime { get; }
    }
}
