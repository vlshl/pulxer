using Platform;
using System.Collections.Generic;

namespace Pulxer.Drawing
{
    public class ChartManagerCache
    {
        private Dictionary<string, ChartManager> _key_chartManager;

        public ChartManagerCache()
        {
            _key_chartManager = new Dictionary<string, ChartManager>();
        }

        public ChartManager GetChartManager(int accountID, int insID, Timeframes tf)
        {
            string key = accountID.ToString()
                + ":" + insID.ToString()
                + ":" + ((byte)tf).ToString();
            if (!_key_chartManager.ContainsKey(key)) return null;
            
            return _key_chartManager[key];
        }

        public bool AddChartManager(int accountID, int insID, Timeframes tf, ChartManager cm)
        {
            string key = accountID.ToString()
                + ":" + insID.ToString()
                + ":" + ((byte)tf).ToString();
            if (_key_chartManager.ContainsKey(key)) return false;
            _key_chartManager.Add(key, cm);

            return true;
        }

        public void Clear()
        {
            _key_chartManager.Clear();
        }
    }
}
