using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public class AuthOptions
    {
        public const string ISSUER = "Pulxer";
        public const string AUDIENCE = "PxClient";
        public const int LIFETIME = 60;
        public const string KEY = "pulxer_secret";

        public static SymmetricSecurityKey GetSymmetricSecurityKey(string key)
        {
            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
                return new SymmetricSecurityKey(hash);
            }
        }
    }
}
