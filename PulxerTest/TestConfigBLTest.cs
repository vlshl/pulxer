using Common.Data;
using Common.Interfaces;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace PulxerTest
{
    public class TestConfigBLTest
    {
        [Fact]
        public void SaveGet_create_get()
        {
            ITestConfigDA da = new TestConfigDAMock();
            ITestConfigBL bl = new TestConfigBL(da);

            TestConfig tc = new TestConfig();
            tc.Name = "name";
            tc.InitialSumma = 1000;
            tc.IsShortEnable = true;
            tc.CommPerc = 0.01m;

            bl.SaveTestConfig(tc);

            var tc1 = bl.GetTestConfig(tc.TestConfigID);

            Assert.Equal(tc.Name, tc1.Name);
            Assert.Equal(tc.InitialSumma, tc1.InitialSumma);
            Assert.Equal(tc.CommPerc, tc1.CommPerc);
            Assert.Equal(tc.IsShortEnable, tc1.IsShortEnable);
        }

        [Fact]
        public void SaveGet_update_get()
        {
            ITestConfigDA da = new TestConfigDAMock();
            ITestConfigBL bl = new TestConfigBL(da);

            TestConfig tc = new TestConfig();
            tc.Name = "name";
            tc.InitialSumma = 1000;
            tc.CommPerc = 0.01m;
            tc.IsShortEnable = true;
            bl.SaveTestConfig(tc);

            Assert.True(tc.TestConfigID > 0);

            tc.Name = "name1";
            tc.InitialSumma = 2000;
            tc.CommPerc = 1.01m;
            tc.IsShortEnable = false;

            bl.SaveTestConfig(tc); // update

            var tc1 = bl.GetTestConfig(tc.TestConfigID);

            Assert.Equal(tc.Name, tc1.Name);
            Assert.Equal(tc.InitialSumma, tc1.InitialSumma);
            Assert.Equal(tc.CommPerc, tc1.CommPerc);
            Assert.Equal(tc.IsShortEnable, tc1.IsShortEnable);
        }

        [Fact]
        public void GetList_create_getList()
        {
            ITestConfigDA da = new TestConfigDAMock();
            ITestConfigBL bl = new TestConfigBL(da);

            TestConfig tc = new TestConfig();
            tc.Name = "name";
            tc.InitialSumma = 1000;
            tc.CommPerc = 0.01m;
            tc.IsShortEnable = true;
            bl.SaveTestConfig(tc);

            var list = bl.GetTestConfigList().ToList();
            Assert.Single(list);
            Assert.Equal(tc.TestConfigID, list[0].TestConfigID);
            Assert.Equal(tc.Name, list[0].Name);
        }

        [Fact]
        public void Delete_createAndDelete_emptyList()
        {
            ITestConfigDA da = new TestConfigDAMock();
            ITestConfigBL bl = new TestConfigBL(da);

            TestConfig tc = new TestConfig();
            tc.Name = "name";
            tc.InitialSumma = 1000;
            tc.CommPerc = 0.01m;
            tc.IsShortEnable = true;
            bl.SaveTestConfig(tc);

            var list = bl.GetTestConfigList().ToList();
            Assert.Single(list);

            bl.DeleteTestConfig(tc.TestConfigID);

            var list1 = bl.GetTestConfigList().ToList();
            Assert.Empty(list1);
        }
    }
}
