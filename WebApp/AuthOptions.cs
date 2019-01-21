using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp
{
    public class AuthOptions
    {
        public const string ISSUER = "Pulxer";
        public const string AUDIENCE = "PxClient";
        public const int LIFETIME = 60;
        public const string KEY = "pulxer_secret";

        public static SymmetricSecurityKey GetSymmetricSecurityKey(string key)
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }
    }
}
