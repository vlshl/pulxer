using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace Storage
{
    /// <summary>
    /// TestConfig da-layer
    /// </summary>
    public class TestConfigDA : ITestConfigDA
    {
        private readonly DbContextOptions<DaContext> _options;

        public TestConfigDA(DbContextOptions<DaContext> options)
        {
            _options = options;
        }

        /// <summary>
        /// Get TestConfig by Id
        /// </summary>
        /// <param name="testConfigID">Id</param>
        /// <returns>TestConfig</returns>
        public DbTestConfig GetTestConfigByID(int testConfigID)
        {
            using (var db = new DaContext(_options))
            {
                return db.TestConfig.Find(testConfigID);
            }
        }

        /// <summary>
        /// Get all TestConfigs
        /// </summary>
        /// <returns>TestConfigs list</returns>
        public IEnumerable<DbTestConfig> GetTestConfigs()
        {
            using (var db = new DaContext(_options))
            {
                return db.TestConfig.ToList();
            }
        }

        /// <summary>
        /// Insert new testConfig
        /// </summary>
        /// <param name="testConfig">TestConfig</param>
        /// <returns>New TestConfig id</returns>
        public int InsertTestConfig(DbTestConfig testConfig)
        {
            using (var db = new DaContext(_options))
            {
                db.TestConfig.Add(testConfig);
                db.SaveChanges();
            }

            return testConfig.TestConfigID;
        }

        /// <summary>
        /// Update TestConfig
        /// </summary>
        /// <param name="testConfig">TestConfig</param>
        public void UpdateTestConfig(DbTestConfig testConfig)
        {
            using (var db = new DaContext(_options))
            {
                db.Update(testConfig);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Delete TestConfig
        /// </summary>
        /// <param name="testConfigID">Id</param>
        public void DeleteTestConfig(int testConfigID)
        {
            using (var db = new DaContext(_options))
            {
                var ts = db.TestConfig.Find(testConfigID);
                if (ts != null)
                {
                    db.TestConfig.Remove(ts);
                    db.SaveChanges();
                }
            }
        }
    }
}
