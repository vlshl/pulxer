using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Platform;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace Storage
{
    public class UserDA : IUserDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public UserDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>User list</returns>
        public IEnumerable<User> GetUsers()
        {
            using (var db = new DaContext(_options))
            {
                return db.User.ToList();
            }
        }

        /// <summary>
        /// Create new user
        /// </summary>
        /// <param name="login">User login</param>
        /// <param name="pwdhash">Password hash string</param>
        /// <param name="role">User role</param>
        /// <returns>New user</returns>
        public User CreateUser(string login, string pwdhash, string role)
        {
            User user = new User()
            {
                UserId = 0,
                Login = login,
                PasswordHash = pwdhash,
                Role = role
            };

            using (var db = new DaContext(_options))
            {
                db.User.Add(user);
                db.SaveChanges();
            }

            return user;
        }

        /// <summary>
        /// Update user data
        /// </summary>
        /// <param name="user">User object</param>
        public void UpdateUser(User user)
        {
            using (var db = new DaContext(_options))
            {
                db.Update<User>(user);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete user by id
        /// </summary>
        /// <param name="userID">User id</param>
        public void DeleteUser(int userID)
        {
            using (var db = new DaContext(_options))
            {
                var user = db.User.Find(userID);
                if (user == null) return;

                db.Remove<User>(user);
                db.SaveChanges();
            }
        }
    }
}
