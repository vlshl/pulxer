using Common.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Storage.Test
{
    public class RepositoryTest
    {
        private readonly DbContextOptions<DaContext> _options;

        public RepositoryTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;
        }

        [Fact]
        public void CreateSelect_()
        {
            var reposDA = new RepositoryDA(_options);

            var r = reposDA.Create("key", "data");
            var r1 = reposDA.Select(0, "key");
            var r2 = reposDA.Select(r.ReposID, null);


            Assert.Equal(r.ReposID, r1.ReposID);
            Assert.Equal(r.Key, r1.Key);
            Assert.Equal(r.Data, r1.Data);
            Assert.Equal(r.ReposID, r2.ReposID);
            Assert.Equal(r.Key, r2.Key);
            Assert.Equal(r.Data, r2.Data);

            // cleanup
            reposDA.Delete(r.ReposID);
        }

        [Fact]
        public void UpdateDelete_()
        {
            // create
            var reposDA = new RepositoryDA(_options);
            var r = reposDA.Create("key", "data");

            r.Data = "data1";
            reposDA.Update(r);

            var r1 = reposDA.Select(0, "key");

            Assert.Equal(r.ReposID, r1.ReposID);
            Assert.Equal(r.Key, r1.Key);
            Assert.Equal(r.Data, r1.Data);

            // delete and cleanup
            reposDA.Delete(r.ReposID);

            var empty = reposDA.Select(r.ReposID, null);

            Assert.Null(empty);
        }
    }
}
