using Common.Data;

namespace Common.Interfaces
{
    public interface ISettingsDA
    {
        Global AddGlobal(string key, string val);
        Global GetGlobal(string key);
        bool UpdateGlobal(Global global);
        bool DeleteGlobal(string key);

        Setting AddSetting(int userId, string category, string key, string val);
        Setting GetSetting(int userId, string category, string key);
        bool UpdateSetting(Setting setting);
        bool DeleteSetting(int userId, string category, string key);
    }
}
