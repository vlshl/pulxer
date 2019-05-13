using System;
using System.Collections.Generic;
using System.Text;

namespace Platform
{
    public class BotParam
    {
        public string Key { get; set; }
        public string Value { get; set; }
        public string Name { get; set; }
        public BotParam[] Children { get; set; }

        public BotParam()
        {
            Key = "";
            Value = "";
            Name = "";
            Children = new BotParam[] { };
        }
    }
}
