using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Data
{
    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
