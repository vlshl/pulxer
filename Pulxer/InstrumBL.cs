using Common.Interfaces;
using Storage;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CommonData = Common.Data;

namespace Pulxer
{
    /// <summary>
    /// Подсистема фин. инструментов
    /// </summary>
    public class InstrumBL : IInstrumBL
    {
        private readonly IInstrumDA _instrumDA;
        private readonly ISettingsBL _settingsBL;
        private readonly IInsStoreDA _insStoreDA;

        public InstrumBL(IInstrumDA instrumDA, ISettingsBL settingsBL, IInsStoreDA insStoreDA)
        {
            _instrumDA = instrumDA;
            _settingsBL = settingsBL;
            _insStoreDA = insStoreDA;
        }

        /// <summary>
        /// Список всех фин. инструментов
        /// </summary>
        /// <returns>Список</returns>
        public IEnumerable<CommonData.InstrumListItem> GetInstrumList()
        {
            return _instrumDA.GetInstrumList();
        }

        private const string FAV_INSTRUM_CATEGORY = "instrum";
        private const string FAV_INSTRUM_KEY = "favorites";

        /// <summary>
        /// Список ID отобранных фин. инструментов
        /// </summary>
        /// <returns>Список идентификаторов</returns>
        public int[] GetFavInstrumIds(int userId)
        {
            var favs = _settingsBL.GetSetting(userId, FAV_INSTRUM_CATEGORY, FAV_INSTRUM_KEY);
            return ParseInts(favs).ToArray();
        }

        /// <summary>
        /// Добавить инструмент в список отобранных для указанного пользователя
        /// </summary>
        /// <param name="userId">Пользователь</param>
        /// <param name="instrumId">Инструмент</param>
        /// <returns>Новый список отобранных</returns>
        public int[] AddFavorite(int userId, int instrumId)
        {
            var favs = _settingsBL.GetSetting(userId, FAV_INSTRUM_CATEGORY, FAV_INSTRUM_KEY);
            var favIds = ParseInts(favs);
            if (favIds.Contains(instrumId)) return favIds.ToArray();

            favIds.Add(instrumId);
            string data = SerializeInts(favIds);
            _settingsBL.SetSetting(userId, FAV_INSTRUM_CATEGORY, FAV_INSTRUM_KEY, data);

            return favIds.ToArray();
        }

        /// <summary>
        /// Удалить инструмент из списка отобранных для указанного пользователя
        /// </summary>
        /// <param name="userId">Пользователь</param>
        /// <param name="instrumId">Инструмент</param>
        /// <returns>Новый список отобранных</returns>
        public int[] RemoveFavorite(int userId, int instrumId)
        {
            var favs = _settingsBL.GetSetting(userId, FAV_INSTRUM_CATEGORY, FAV_INSTRUM_KEY);
            var favIds = ParseInts(favs);
            if (!favIds.Contains(instrumId)) return favIds.ToArray();

            favIds.Remove(instrumId);
            string data = SerializeInts(favIds);
            _settingsBL.SetSetting(userId, FAV_INSTRUM_CATEGORY, FAV_INSTRUM_KEY, data);

            return favIds.ToArray();
        }

        private List<int> ParseInts(string str)
        {
            string[] parts = Regex.Split(str, @"\s*;\s*");
            List<int> vals = new List<int>();
            int r;
            foreach (var p in parts)
            {
                if (int.TryParse(p, out r)) { vals.Add(r); }
            }

            return vals;
        }

        private string SerializeInts(List<int> ints)
        {
            return string.Join(';', ints);
        }

        /// <summary>
        /// Получить список активных инструментов, т.е. тех, на которые накапливаются исторические данные
        /// </summary>
        /// <returns>Список идентификаторов</returns>
        public int[] GetActiveInstrumIds()
        {
            return _insStoreDA.GetInsStores(null, null, true)
                .Select(r => r.InsID)
                .Distinct().ToArray(); ;
        }

        /// <summary>
        /// Список всех фин. инструментов
        /// </summary>
        /// <returns>Список</returns>
        public IEnumerable<CommonData.Instrum> GetInstrums()
        {
            return _instrumDA.GetInstrums();
        }

        /// <summary>
        /// Данные фин. инструмента по ID
        /// </summary>
        /// <param name="insID">ID инструмента</param>
        /// <returns>Фин. инструмент (новый, если 0 или отрицательное)</returns>
        public CommonData.Instrum GetInstrumByID(int insID)
        {
            return _instrumDA.GetInstrum(insID);
        }

        /// <summary>
        /// Данные фин. инструмента по ticker
        /// </summary>
        /// <param name="ticker">Тикер</param>
        /// <returns>Фин. инструмент (null, если тикер не найден)</returns>
        public CommonData.Instrum GetInstrum(string ticker)
        {
            return _instrumDA.GetInstrum(0, ticker);
        }

        /// <summary>
        /// Удаление фин. инструмента по ID
        /// </summary>
        /// <param name="insID">ID фин. инструмента</param>
        public void DeleteInstrumByID(int insID)
        {
            _instrumDA.DeleteInstrumByID(insID);
        }
    }
}
