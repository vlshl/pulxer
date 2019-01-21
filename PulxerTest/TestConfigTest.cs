using Common;
using Common.Data;
using Common.Interfaces;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PulxerTest
{
    public class TestConfigTest
    {
        [Fact]
        public void SerializeInitialize_object_equalObject()
        {
            TestConfig te = new TestConfig();
            te.TestConfigID = 100;
            te.Name = "name";
            te.InitialSumma = 1000;
            te.CommPerc = 0.5m;
            te.IsShortEnable = true;
            te.AddBotConfig("k1", "a1", "c1", "d1");
            te.AddBotConfig("k2", "a2", "c2", "d2");
            te.AddBotConfig("k3", "a3", "c3", "d3");

            var xd = te.Serialize();
            var te1 = new TestConfig();
            te1.Initialize(xd);

            Assert.Equal(te.InitialSumma, te1.InitialSumma);
            Assert.Equal(te.CommPerc, te1.CommPerc);
            Assert.Equal(te.IsShortEnable, te1.IsShortEnable);
            var botConfigs = te1.GetBotConfigs().ToList();
            Assert.Equal(3, botConfigs.Count);
            var b0 = botConfigs[0];
            var b1 = botConfigs[1];
            var b2 = botConfigs[2];
            Assert.Equal("k1", b0.Key);
            Assert.Equal("a1", b0.Assembly);
            Assert.Equal("c1", b0.Class);
            Assert.Equal("d1", b0.InitData);

            Assert.Equal("k2", b1.Key);
            Assert.Equal("a2", b1.Assembly);
            Assert.Equal("c2", b1.Class);
            Assert.Equal("d2", b1.InitData);

            Assert.Equal("k3", b2.Key);
            Assert.Equal("a3", b2.Assembly);
            Assert.Equal("c3", b2.Class);
            Assert.Equal("d3", b2.InitData);
        }

        [Fact]
        public void AddRemoveBotConfig_addingAndRemoving_correct()
        {
            TestConfig te = new TestConfig();
            te.TestConfigID = 100;
            te.Name = "name";
            te.InitialSumma = 1000;
            te.CommPerc = 0.5m;
            te.IsShortEnable = true;

            Assert.Empty(te.GetBotConfigs()); // в начале список пустой

            te.AddBotConfig("k1", "a1", "c1", "d1");
            Assert.Single(te.GetBotConfigs()); // один добавили

            te.AddBotConfig("k1", "a2", "c2", "d2"); // дублирование ключа - не добавляется
            Assert.Single(te.GetBotConfigs()); // все равно в списке один элемент

            te.AddBotConfig("k2", "a2", "c2", "d2"); // теперь добавляется
            Assert.Equal(2, te.GetBotConfigs().Count()); // в списке два элемента

            te.RemoveBotConfig("k3"); // нет такого ключа, поэтому ничего не удаляется
            Assert.Equal(2, te.GetBotConfigs().Count()); // в списке два элемента

            te.RemoveBotConfig("k2"); // удаляем
            Assert.Single(te.GetBotConfigs()); // остался одни элемент

            te.RemoveBotConfig("k1"); // удаляем последний
            Assert.Empty(te.GetBotConfigs()); // в списке пусто
        }

        [Fact]
        public void AddRemoveBotConfig_addAndRemoveByIncorrectKey_ignoreKey()
        {
            TestConfig te = new TestConfig();
            te.TestConfigID = 100;
            te.Name = "name";
            te.InitialSumma = 1000;
            te.CommPerc = 0.5m;
            te.IsShortEnable = true;

            Assert.Empty(te.GetBotConfigs()); // в начале список пустой

            te.AddBotConfig("", "a1", "c1", "d1");
            Assert.Empty(te.GetBotConfigs()); // с пустым ключом не добавляем

            te.AddBotConfig(null, "a1", "c1", "d1");
            Assert.Empty(te.GetBotConfigs()); // с пустым ключом не добавляем

            te.AddBotConfig("k1", "a1", "c1", "d1"); // один добавили

            te.RemoveBotConfig(""); // ничего не удаляем
            te.RemoveBotConfig(null); // и так тоже ничего не удаляем
            Assert.Single(te.GetBotConfigs()); // в списке один элемент
        }

        [Fact]
        public void AddBotConfig_addIncorrectData_correctData()
        {
            TestConfig te = new TestConfig();
            te.TestConfigID = 100;
            te.Name = "name";
            te.InitialSumma = 1000;
            te.CommPerc = 0.5m;
            te.IsShortEnable = true;

            Assert.Empty(te.GetBotConfigs()); // в начале список пустой

            te.AddBotConfig("k1", "", "", "");
            te.AddBotConfig("k2", null, null, null);
            var confs = te.GetBotConfigs().ToList();
            Assert.Equal(2, confs.Count); // в списке два элемента
            Assert.Equal("k1", confs[0].Key); // все null исправлены на пустые строки
            Assert.Equal("", confs[0].Assembly);
            Assert.Equal("", confs[0].Class);
            Assert.Equal("", confs[0].InitData);
            Assert.Equal("k2", confs[1].Key);
            Assert.Equal("", confs[1].Assembly);
            Assert.Equal("", confs[1].Class);
            Assert.Equal("", confs[1].InitData);
        }
    }
}
