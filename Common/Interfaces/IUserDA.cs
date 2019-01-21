using Common.Data;
using System.Collections.Generic;

namespace Common.Interfaces
{
    public interface IUserDA
    {
        IEnumerable<User> GetUsers();
        User CreateUser(string login, string pwdhash, string role);
        void UpdateUser(User user);
        void DeleteUser(int userID);
    }
}
