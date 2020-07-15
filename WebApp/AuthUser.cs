using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp
{
    public class AuthUser
    {
        public int UserID { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public string ExpTimeStr { get; set; }

        public AuthUser()
        {
            Login = Role = Token = ExpTimeStr = "";
        }
    }
}
