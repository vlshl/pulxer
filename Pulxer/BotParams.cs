using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulxer
{
    public class BotParams : IBotParams
    {
        private IEnumerable<BotParam> _params;

        public BotParams(IEnumerable<BotParam> prms)
        {
            _params = prms;
        }

        public BotParam GetBotParam(string key)
        {
            if (_params == null) return null;

            if (key.Contains('/'))
            {
                string[] keys = key.Split('/');
                IEnumerable<BotParam> prms = _params;

                BotParam found = null;
                foreach (var k in keys)
                {
                    if (string.IsNullOrWhiteSpace(k)) continue;

                    found = prms.FirstOrDefault(p => p.Key == k);
                    if (found == null) return null;

                    prms = found.Children;
                }

                return found;
            }
            else
            {
                return _params.FirstOrDefault(p => p.Key == key);
            }
        }

        public int GetIntValue(string key)
        {
            var p = GetBotParam(key);
            if (p == null) return 0;

            int res;
            if (int.TryParse(p.Value, out res)) return res;

            return 0;

        }

        public decimal GetDecValue(string key)
        {
            var p = GetBotParam(key);
            if (p == null) return 0m;

            decimal res;
            if (decimal.TryParse(p.Value, out res)) return res;

            return 0m;

        }

        public string GetValue(string key)
        {
            var p = GetBotParam(key);
            if (p == null) return "";

            return p.Value;
        }
    }
}
