using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Storage
{
    public class SettingsDA : ISettingsDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public SettingsDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Добавить новую глобальную настройку
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <param name="val">Значение</param>
        /// <returns>Созданный объект</returns>
        public Global AddGlobal(string key, string val)
        {
            Global g = new Global()
            {
                GlobalId = 0,
                Key = key,
                Value = val
            };

            using (var db = new DaContext(_options))
            {
                db.Globals.Add(g);
                int count = db.SaveChanges();
                if (count != 1) return null;
            }

            return g;
        }

        /// <summary>
        /// Получить глобальную настройку по ключу
        /// </summary>
        /// <param name="key">Уникальный ключ</param>
        /// <returns></returns>
        public Global GetGlobal(string key)
        {
            using (var db = new DaContext(_options))
            {
                return db.Globals.FirstOrDefault(r => r.Key == key);
            }
        }

        /// <summary>
        /// Обновить глобальную настройку
        /// </summary>
        /// <param name="global">Объект настройки</param>
        /// <returns>true - запись обновлена</returns>
        public bool UpdateGlobal(Global global)
        {
            using (var db = new DaContext(_options))
            {
                db.Globals.Update(global);
                return db.SaveChanges() == 1;
            }
        }

        /// <summary>
        /// Удаление глобальной настройки
        /// </summary>
        /// <param name="key">Ключ</param>
        /// <returns>true - запись удалена</returns>
        public bool DeleteGlobal(string key)
        {
            using (var db = new DaContext(_options))
            {
                var g = db.Globals.FirstOrDefault(r => r.Key == key);
                if (g == null) return false;

                db.Globals.Remove(g);
                return db.SaveChanges() == 1;
            }
        }

        /// <summary>
        /// Добавить новую пользовательскую настройку
        /// </summary>
        /// <param name="userId">Пользователь</param>
        /// <param name="category">Категория</param>
        /// <param name="key">Ключ</param>
        /// <param name="val">Значение</param>
        /// <returns>Созданный объект</returns>
        public Setting AddSetting(int userId, string category, string key, string val)
        {
            var s = new Setting()
            {
                SettingId = 0,
                UserId = userId,
                Category = category,
                Key = key,
                Value = val
            };

            using (var db = new DaContext(_options))
            {
                db.Settings.Add(s);
                int count = db.SaveChanges();
                if (count != 1) return null;
            }

            return s;
        }

        /// <summary>
        /// Получить пользовательскую настройку по категории и ключу
        /// </summary>
        /// <param name="userId">Пользователь</param>
        /// <param name="category">Категория</param>
        /// <param name="key">Ключ</param>
        /// <returns>Объект настройки</returns>
        public Setting GetSetting(int userId, string category, string key)
        {
            using (var db = new DaContext(_options))
            {
                return db.Settings.FirstOrDefault(r => r.UserId == userId && r.Category == category && r.Key == key);
            }
        }

        /// <summary>
        /// Обновить пользовательскую настройку
        /// </summary>
        /// <param name="setting">Объект настройки</param>
        /// <returns>true - успешно</returns>
        public bool UpdateSetting(Setting setting)
        {
            using (var db = new DaContext(_options))
            {
                db.Settings.Update(setting);
                return db.SaveChanges() == 1;
            }
        }

        /// <summary>
        /// Удаление пользовательской настройки
        /// </summary>
        /// <param name="userId">Пользователь</param>
        /// <param name="category">Категория</param>
        /// <param name="key">Ключ</param>
        /// <returns>true - успешно</returns>
        public bool DeleteSetting(int userId, string category, string key)
        {
            using (var db = new DaContext(_options))
            {
                var s = db.Settings.FirstOrDefault(r => r.UserId == userId && r.Category == category && r.Key == key);
                if (s == null) return false;

                db.Settings.Remove(s);
                return db.SaveChanges() == 1;
            }
        }
    }
}
