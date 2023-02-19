namespace Common.Interfaces
{
    public interface ISettingsBL
    {
        string GetGlobal(string key);
        void SetGlobal(string key, string val);
        string GetSetting(int userId, string category, string key);
        void SetSetting(int userId, string category, string key, string val);
    }
}
