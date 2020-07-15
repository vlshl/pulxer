using Common.Data;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface IUserBL
    {
        User CreateUser(string login, string password, string role);
        IEnumerable<User> GetUsers();
        bool SetPassword(string login, string password);
        bool DeleteUser(string login);
        string CalcHash(string text);
        User AuthUser(string login, string password);
        string BuildJwtToken(IConfiguration config, User user, out DateTime expTime);
    }
}
