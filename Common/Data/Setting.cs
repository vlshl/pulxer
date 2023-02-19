using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    public class Setting
    {
        public int SettingId { get; set; }
        public int UserId { get; set; }
        public string Category { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
