using Pulxer.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class RemoteVisual
    {
        public int Key { get; set; }
        public string Side { get; set; }
        public string Type { get; set; }

        public RemoteVisual(int key, string side, string type)
        {
            Key = key;
            Side = side;
            Type = type;
        }
    }
}
