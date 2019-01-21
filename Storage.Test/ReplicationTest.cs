using Common.Data;
using Microsoft.EntityFrameworkCore;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Storage.Test
{
    public class ReplicationTest
    {
        private readonly DbContextOptions<DaContext> _options;
        private readonly ReplicationDA _repDA;

        public ReplicationTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;

            _repDA = new ReplicationDA(_options);
        }

        [Fact]
        public void Replication_()
        {
            Dictionary<int, int> replDic = new Dictionary<int, int>();
            replDic.Add(10000, 10);
            _repDA.InsertReplications(ReplObjects.Instrum, replDic);
            var repls = _repDA.GetReplications(ReplObjects.Instrum).ToList();

            Assert.Single(repls);
            Assert.Equal(10, repls[0].LocalID);
            Assert.Equal(10000, repls[0].RemoteID);

            _repDA.DeleteReplications(ReplObjects.Instrum);
            var repls1 = _repDA.GetReplications(ReplObjects.Instrum);

            Assert.Empty(repls1);
        }
    }
}
