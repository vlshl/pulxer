using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CommonData = Common.Data;

namespace Pulxer
{
    public interface ITestConfigBL
    {
        IEnumerable<CommonData.TestConfigItem> GetTestConfigList();
        TestConfig GetTestConfig(int testConfigID);
        void SaveTestConfig(TestConfig testConfig);
        void DeleteTestConfig(int testConfigID);
    }

    /// <summary>
    /// Подсистема источников тиковых данных
    /// </summary>
    public class TestConfigBL : ITestConfigBL
    {
        private readonly ITestConfigDA _testConfigDA = null;

        public TestConfigBL(ITestConfigDA testConfigDA)
        {
            _testConfigDA = testConfigDA;
        }

        /// <summary>
        /// Список всех тестовых конфигураций
        /// </summary>
        /// <returns>Список</returns>
        public IEnumerable<CommonData.TestConfigItem> GetTestConfigList()
        {
            var list = _testConfigDA.GetTestConfigs().ToList();
            return list.Select(r => new TestConfigItem(r.TestConfigID, r.Name));
        }

        /// <summary>
        /// Тестовая конфигурация
        /// </summary>
        /// <param name="testConfigID">ID</param>
        /// <returns>Тестовая конфигурация или null</returns>
        public TestConfig GetTestConfig(int testConfigID)
        {
            if (testConfigID <= 0) return null;

            try
            {
                var tc = _testConfigDA.GetTestConfigByID(testConfigID);
                if (tc == null || tc.DataStr == null) return null;

                var testConfig = new TestConfig();
                testConfig.TestConfigID = tc.TestConfigID;
                testConfig.Name = tc.Name;
                XDocument xd = XDocument.Parse(tc.DataStr);
                testConfig.Initialize(xd);

                return testConfig;
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при извлечении тестовой конфигурации", ex);
            }
        }

        /// <summary>
        /// Сохранение тестовой конфигурации
        /// </summary>
        /// <param name="testConfig">Тестовая крнфигурация</param>
        public void SaveTestConfig(TestConfig testConfig)
        {
            if (testConfig == null)
                throw new ArgumentNullException("testConfig");

            string xml = "";
            var xd = testConfig.Serialize();
            if (xd != null) xml = xd.ToString(SaveOptions.DisableFormatting);

            DbTestConfig tc = new DbTestConfig() { TestConfigID = testConfig.TestConfigID, Name = testConfig.Name, DataStr = xml };

            try
            {
                if (testConfig.TestConfigID > 0)
                {
                    _testConfigDA.UpdateTestConfig(tc);
                }
                else
                {
                    testConfig.TestConfigID = _testConfigDA.InsertTestConfig(tc);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при сохранении тестовой конфигурации", ex);
            }
        }

        /// <summary>
        /// Удаление тестовой конфигурации
        /// </summary>
        /// <param name="testConfigID">Идентификатор</param>
        public void DeleteTestConfig(int testConfigID)
        {
            try
            {
                _testConfigDA.DeleteTestConfig(testConfigID);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при удалении тестовой конфигурации", ex);
            }
        }
    }
}
