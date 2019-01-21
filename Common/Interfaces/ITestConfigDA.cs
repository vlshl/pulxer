using Common.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface ITestConfigDA
    {
        DbTestConfig GetTestConfigByID(int testConfigID);
        IEnumerable<DbTestConfig> GetTestConfigs();
        int InsertTestConfig(DbTestConfig testConfig);
        void UpdateTestConfig(DbTestConfig testConfig);
        void DeleteTestConfig(int testConfigID);
    }
}
