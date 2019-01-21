using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulxerTest
{
    public class TestConfigDAMock : ITestConfigDA
    {
        private Dictionary<int, DbTestConfig> _id_testConfig;

        public TestConfigDAMock()
        {
            _id_testConfig = new Dictionary<int, DbTestConfig>();
        }

        public void DeleteTestConfig(int testConfigID)
        {
            if (_id_testConfig.ContainsKey(testConfigID))
            {
                _id_testConfig.Remove(testConfigID);
            }
        }

        public DbTestConfig GetTestConfigByID(int testConfigID)
        {
            if (_id_testConfig.ContainsKey(testConfigID))
                return _id_testConfig[testConfigID];
            else
                return null;
        }

        public IEnumerable<DbTestConfig> GetTestConfigs()
        {
            return _id_testConfig.Values.ToList();
        }

        public int InsertTestConfig(DbTestConfig testConfig)
        {
            if (testConfig.TestConfigID != 0) return 0;

            int id = 0;
            if (_id_testConfig.Keys.Any())
            {
                id = _id_testConfig.Keys.Max() + 1;
            }
            else
            {
                id = 1;
            }

            testConfig.TestConfigID = id;
            _id_testConfig.Add(id, testConfig);

            return testConfig.TestConfigID;
        }

        public void UpdateTestConfig(DbTestConfig testConfig)
        {
            if (testConfig.TestConfigID <= 0) return;
            if (!_id_testConfig.Keys.Contains(testConfig.TestConfigID)) return;

            _id_testConfig[testConfig.TestConfigID] = testConfig;
        }
    }
}
