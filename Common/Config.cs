using Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Common
{
    public class Config : IConfig
    {
        private IConfigurationSection _section;

        public Config(IConfigurationSection section)
        {
            _section = section;
        }

        /// <summary>
        /// Путь к данным всех сделок (путь к каталогам с датами)
        /// </summary>
        /// <returns></returns>
        public string GetLeechDataPath()
        {
            return _section["LeechDataPath"];
        }

        public string GetHistoryProviderConfig()
        {
            return _section["HistoryProviderConfig"];
        }

        public string GetHistoryProviderCache()
        {
            return _section["HistoryProviderCache"];
        }

        public string GetBotsPath()
        {
            return _section["BotsPath"];
        }
    }
}
