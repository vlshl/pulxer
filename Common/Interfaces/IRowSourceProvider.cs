using System.Collections.Generic;

namespace Common.Interfaces
{
    /// <summary>
    /// ValueRowSourceProvider interface
    /// ValueRowSourcesProvider provides ValueRowSource objects
    /// </summary>
    public interface IValueRowSourcesProvider
    {
        void Initialize(IEnumerable<ValueRowSource> srcs);
        void AddSources(string indicID, IEnumerable<ValueRowSource> srcs);
        void RemoveSources(string indicID);
        IEnumerable<ValueRowSource> GetSources(string indicID);
        ValueRowSource GerSourceByID(string sourceID);
    }
}
