using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class Lib
    {
        public static string Ids2Str(IEnumerable<int> ids)
        {
            return string.Join(",", ids.Select(r => r.ToString()));
        }

        public static IEnumerable<int> Str2Ids(string str)
        {
            if (str == null) return null;
            if (str == "") return new List<int>();

            var parts = str.Split(',');
            List<int> list = new List<int>();
            int n;
            foreach (var part in parts)
            {
                if (int.TryParse(part, out n))
                {
                    list.Add(n);
                }
            }

            return list;
        }
    }
}
