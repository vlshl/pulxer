using Common;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pulxer
{
    /// <summary>
    /// Провайдер именованных источников ValueRow.
    /// ValueRow - массив значений с привязкой к временнОй оси.
    /// </summary>
    public class ValueRowSourceProvider : IValueRowSourcesProvider
    {
        private List<string> _indicIDs;
        private List<Tuple<string, ValueRowSource>> _indicID_src;

        public ValueRowSourceProvider()
        {
            _indicIDs = new List<string>();
            _indicID_src = new List<Tuple<string, ValueRowSource>>();
        }

        /// <summary>
        /// Добавить источники в провайдер.
        /// Каждый индикатор на графике может быть источником нескольких массивов данных, привязанных к временнОй оси.
        /// Эти источники могут использоваться другими индикаторами, которые будут добавлены в график позднее.
        /// </summary>
        /// <param name="indicID">Строковый идентификатор индикатора</param>
        /// <param name="srcs">Источники массивов данных</param>
        public void AddSources(string indicID, IEnumerable<ValueRowSource> srcs)
        {
            _indicIDs.Add(indicID);
            foreach (var src in srcs) _indicID_src.Add(new Tuple<string, ValueRowSource>(indicID, src));
        }

        /// <summary>
        /// Получить именованный массив данных по идентификатору
        /// </summary>
        /// <param name="sourceID">Идентификатор именованного массива</param>
        /// <returns>Именованный массив значение, привязанный к временной шкале</returns>
        public ValueRowSource GerSourceByID(string sourceID)
        {
            var found = _indicID_src.FirstOrDefault(t => t.Item2.Guid == sourceID);
            if (found == null) return null;
            return found.Item2;
        }

        /// <summary>
        /// Получить список именованных массивов по идентификатору индикатора
        /// </summary>
        /// <param name="id">Идентификатор индикатора на графике</param>
        /// <returns>Список именованных массивов данных</returns>
        public IEnumerable<ValueRowSource> GetSources(string id)
        {
            var idx = _indicIDs.FindIndex(s => s == id);
            if (idx <= 0) return null;

            var listIDs = _indicIDs.Take(idx).ToArray();

            return _indicID_src
                .Where(s => listIDs.Contains(s.Item1))
                .Select(s => s.Item2).ToArray();
        }

        /// <summary>
        /// Инициализация провайдера. Создание списка именованных массивов данных.
        /// </summary>
        /// <param name="srcs">Список массивов</param>
        public void Initialize(IEnumerable<ValueRowSource> srcs)
        {
            _indicID_src.Clear();
            foreach (var src in srcs) _indicID_src.Add(new Tuple<string, ValueRowSource>("", src));
            _indicIDs.Clear();
            _indicIDs.Add("");
        }

        /// <summary>
        /// Удаление индикатора и всех источников, которые он выдает
        /// </summary>
        /// <param name="id">Идентификатор индикатора</param>
        public void RemoveSources(string id)
        {
            _indicIDs.Remove(id);
            _indicID_src.RemoveAll(s => s.Item1 == id);
        }
    }
}
