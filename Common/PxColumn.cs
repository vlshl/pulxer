using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class PxColumn
    {
        public PxColumn(string key, string name, string type = "string") 
        {
            Key = key;
            Name = name;
            Type = type;
        }

        public string Key { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
