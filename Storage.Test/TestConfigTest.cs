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
    public class TestConfigTest
    {
        private readonly DbContextOptions<DaContext> _options;
        private readonly TestConfigDA _testConfigDA;

        public TestConfigTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;

            _testConfigDA = new TestConfigDA(_options);
        }

        [Fact]
        public void TestConfig_()
        {
            // создание TestConfig
            DbTestConfig tc = new DbTestConfig();
            tc.Name = "name"; tc.DataStr = "datastr";
            int tcID = _testConfigDA.InsertTestConfig(tc);

            // вывод списка TestConfig
            var list = _testConfigDA.GetTestConfigs();

            Assert.Single(list.Where(r => r.TestConfigID == tcID));

            // вывод testConfig
            var testConfig = _testConfigDA.GetTestConfigByID(list.ElementAt(0).TestConfigID);

            Assert.Equal(tcID, testConfig.TestConfigID);
            Assert.Equal("name", testConfig.Name);
            Assert.Equal("datastr", testConfig.DataStr);

            // изменение
            testConfig.Name = "name1"; testConfig.DataStr = "datastr1";
            _testConfigDA.UpdateTestConfig(testConfig);

            // вывод измененного testConfig
            var testConfig1 = _testConfigDA.GetTestConfigByID(testConfig.TestConfigID);

            Assert.Equal(tcID, testConfig.TestConfigID);
            Assert.Equal("name1", testConfig.Name);
            Assert.Equal("datastr1", testConfig.DataStr);

            // удаление
            _testConfigDA.DeleteTestConfig(testConfig.TestConfigID);

            // должен быть пустой список
            var list1 = _testConfigDA.GetTestConfigs();

            Assert.Empty(list1);
        }

    }
}
