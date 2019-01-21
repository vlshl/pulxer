using Common.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Storage.Test
{
    public class UserTest
    {
        private readonly DbContextOptions<DaContext> _options;

        public UserTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;
        }

        [Fact]
        public void CreateGet_()
        {
            var userDA = new UserDA(_options);
            var user = userDA.CreateUser("login", "hash", "role");
            var users = userDA.GetUsers().ToList();

            Assert.Single(users);
            var user1 = users[0];

            Assert.Equal(user.UserID, user1.UserID);
            Assert.Equal(user.Login, user1.Login);
            Assert.Equal(user.PasswordHash, user1.PasswordHash);
            Assert.Equal(user.Role, user1.Role);

            // cleanup
            userDA.DeleteUser(user.UserID);
        }

        [Fact]
        public void UpdateDelete_()
        {
            int userID;

            // create
            var userDA = new UserDA(_options);
            userID = userDA.CreateUser("login", "hash", "role").UserID;

            var user = userDA.GetUsers().FirstOrDefault(r => r.UserID == userID);

            user.Login = "login1";
            user.PasswordHash = "hash1";
            user.Role = "role1";

            userDA.UpdateUser(user);

            var user1 = userDA.GetUsers().FirstOrDefault(r => r.UserID == userID);

            Assert.Equal(userID, user1.UserID);
            Assert.Equal("login1", user1.Login);
            Assert.Equal("hash1", user1.PasswordHash);
            Assert.Equal("role1", user1.Role);

            // delete and cleanp
            userDA.DeleteUser(userID);

            var emptyList = userDA.GetUsers();

            Assert.Empty(emptyList);
        }
    }
}
