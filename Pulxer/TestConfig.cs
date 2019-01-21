using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Pulxer
{
    public class TestConfig
    {
        private List<BotConfig> _botConfigs;

        public TestConfig()
        {
            _botConfigs = new List<BotConfig>();

            Name = "";
            InitialSumma = 0;
            CommPerc = 0;
            IsShortEnable = false;
        }

        /// <summary>
        /// Identifier
        /// </summary>
        public int TestConfigID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        public decimal InitialSumma { get; set; }

        public decimal CommPerc { get; set; }

        public bool IsShortEnable { get; set; }

        public void AddBotConfig(string key, string assembly, string cls, string initData)
        {
            if (string.IsNullOrEmpty(key)) return;
            if (assembly == null) assembly = "";
            if (cls == null) cls = "";
            if (initData == null) initData = "";

            if (_botConfigs.Any(r => r.Key == key)) return;

            var bc = new BotConfig()
            {
                Key = key,
                Assembly = assembly,
                Class = cls,
                InitData = initData
            };
            _botConfigs.Add(bc);
        }

        public void RemoveBotConfig(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            var found = _botConfigs.FirstOrDefault(r => r.Key == key);
            if (found != null) _botConfigs.Remove(found);
        }

        public IEnumerable<BotConfig> GetBotConfigs()
        {
            return _botConfigs.ToList();
        }

        public void Initialize(XDocument xDoc)
        {
            var xnRoot = xDoc.Element("TestConfig");
            if (xnRoot == null) return;

            try
            {
                var sum = xnRoot.Attribute("InitialSumma").Value;
                InitialSumma = decimal.Parse(sum);

                var perc = xnRoot.Attribute("CommPerc").Value;
                CommPerc = decimal.Parse(perc);

                var se = xnRoot.Attribute("IsShortEnable").Value;
                IsShortEnable = se == "1";

                _botConfigs.Clear();

                var xnBots = xnRoot.Element("Bots");
                foreach (var xnBot in xnBots.Elements("Bot"))
                {
                    string key = xnBot.Attribute("Key").Value;
                    string assembly = xnBot.Attribute("Assembly").Value;
                    string cls = xnBot.Attribute("Class").Value;
                    string initData = xnBot.Attribute("InitData").Value;
                    AddBotConfig(key, assembly, cls, initData);
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка при создании TestConfig", ex);
            }
        }

        public XDocument Serialize()
        {
            XDocument xd = new XDocument(new XElement("TestConfig"));
            xd.Root.Add(new XAttribute("InitialSumma", InitialSumma.ToString()));
            xd.Root.Add(new XAttribute("CommPerc", CommPerc.ToString()));
            xd.Root.Add(new XAttribute("IsShortEnable", IsShortEnable ? "1" : "0"));

            var xeBots = new XElement("Bots");
            xd.Root.Add(xeBots);

            foreach (var bc in _botConfigs)
            {
                var xeBot = new XElement("Bot");
                xeBot.Add(new XAttribute("Key", bc.Key));
                xeBot.Add(new XAttribute("Assembly", bc.Assembly));
                xeBot.Add(new XAttribute("Class", bc.Class));
                xeBot.Add(new XAttribute("InitData", bc.InitData));
                xeBots.Add(xeBot);
            }

            return xd;
        }
    }
}
