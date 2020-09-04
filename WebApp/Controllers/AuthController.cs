using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IUserBL _userBL;
        private readonly IConfiguration _config;

        public AuthController(IUserBL userBL, IConfiguration config)
        {
            _userBL = userBL;
            _config = config;
        }

        [HttpPost("/auth")]
        public ActionResult<AuthUser> Auth(string login, string password)
        {
            var user = _userBL.AuthUser(login, password);
            if (user == null) return Unauthorized();

            DateTime expTime;
            string token = _userBL.BuildJwtToken(_config, user, out expTime);

            AuthUser authUser = new AuthUser()
            {
                UserID = user.UserID,
                Login = user.Login,
                Token = token,
                ExpTimeStr = expTime.ToString("o"),
                Role = user.Role
            };

            return Ok(authUser);
        }

        // оставлено для совместимости с мобильным клиентом
        [HttpPost("/token")]
        public async Task Token()
        {
            var username = Request.Form["username"];
            var password = Request.Form["password"];

            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                Response.StatusCode = 400;
                await Response.WriteAsync("Invalid login or password.");
                return;
            }

            var jwtConfig = _config.GetSection("JwtToken");
            string key = DataProtect.TryUnProtect(jwtConfig.GetValue("Key", AuthOptions.KEY));
            string issuer = jwtConfig.GetValue("Issuer", AuthOptions.ISSUER);
            string audience = jwtConfig.GetValue("Audience", AuthOptions.AUDIENCE);
            int lifetime = jwtConfig.GetValue("Lifetime", AuthOptions.LIFETIME);
            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(lifetime)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name
            };

            Response.ContentType = "application/json";
            await Response.WriteAsync(JsonConvert.SerializeObject(response,
                new JsonSerializerSettings { Formatting = Formatting.Indented }));
        }

        private ClaimsIdentity GetIdentity(string login, string password)
        {
            string hash = _userBL.CalcHash(password);
            User user = _userBL.GetUsers().FirstOrDefault(r =>
                r.Login == login && r.PasswordHash == hash);
            if (user == null) return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }
    }
}