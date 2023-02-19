using Common.Interfaces;

namespace Pulxer
{
    public class SettingsBL : ISettingsBL
    {
        private readonly ISettingsDA _settingsDA;

        public SettingsBL(ISettingsDA settingsDA)
        {
            _settingsDA = settingsDA;
        }

        public string GetGlobal(string key)
        {
            var g = _settingsDA.GetGlobal(key);
            return g != null ? g.Value : "";
        }

        public void SetGlobal(string key, string val)
        {
            var g = _settingsDA.GetGlobal(key);
            if (g == null)
            {
                _settingsDA.AddGlobal(key, val);
            }
            else
            {
                g.Value = val;
                _settingsDA.UpdateGlobal(g);
            }
        }

        public string GetSetting(int userId, string category, string key)
        {
            var s = _settingsDA.GetSetting(userId, category, key);
            return s != null ? s.Value : "";
        }

        public void SetSetting(int userId, string category, string key, string val)
        {
            var s = _settingsDA.GetSetting(userId, category, key);
            if (s == null)
            {
                _settingsDA.AddSetting(userId, category, key, val);
            }
            else
            {
                s.Value = val;
                _settingsDA.UpdateSetting(s);
            }
        }
    }
}
