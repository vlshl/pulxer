using CommonData = Common.Data;

namespace Common.Interfaces
{
    public interface IRepositoryDA
    {
        CommonData.ReposObject Create(string key, string data);
        CommonData.ReposObject Select(int reposID = 0, string key = null);
        void Update(CommonData.ReposObject ro);
        void Delete(int reposID);
    }
}
