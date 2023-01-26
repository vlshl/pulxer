using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUserBL _userBL;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;

        public AuthController(IUserBL userBL, IConfiguration config, ILogger<AuthController> logger)
        {
            _userBL = userBL;
            _config = config;
            _logger = logger;
        }

        [HttpPost("user")]
        public ActionResult<AuthUser> AuthUser(string login, string password)
        {
            var user = _userBL.AuthUser(login, password);
            if (user == null)
            {
                _logger.LogInformation("Logon failed: {0}", login);
                return Unauthorized();
            }

            DateTime expTime;
            string token = _userBL.BuildJwtToken(_config, user, out expTime);

            AuthUser authUser = new AuthUser()
            {
                UserID = user.UserId,
                Login = user.Login,
                Token = token,
                ExpTimeStr = expTime.ToString("o"),
                Role = user.Role
            };

            _logger.LogInformation("Logon success: {0}", user.Login);

            return Ok(authUser);
        }

        [HttpPost("device")]
        public ActionResult<AuthUser> AuthDevice(string devUid, string code)
        {
            var dev = _userBL.AuthDevice(devUid, code);
            if (dev == null)
            {
                _logger.LogInformation("Device logon failed: {0}", devUid);
                return Unauthorized();
            }

            var user = _userBL.GetUserById(dev.UserId);
            if (user == null)
            {
                _logger.LogInformation("User not found: {0}", dev.UserId.ToString());
                return Unauthorized();
            }

            DateTime expTime;
            string token = _userBL.BuildJwtToken(_config, user, out expTime);

            AuthUser authUser = new AuthUser()
            {
                UserID = user.UserId,
                Login = user.Login,
                Token = token,
                ExpTimeStr = expTime.ToString("o"),
                Role = user.Role
            };

            _logger.LogInformation("Device logon success: Login={0}, Uid={1}", user.Login, dev.Uid);

            return Ok(authUser);
        }

        [HttpPost("createdev")]
        [Produces("application/json")] // response
        public string CreateDevice(string login, string password, string code)
        {
            string res = _userBL.CreateDeviceAuth(login, password, code);
            if (string.IsNullOrEmpty(res)) return "";

            return res;
        }

        [HttpPost("deletedev")]
        public bool DeleteDevice(string login, string password, string devUid)
        {
            return _userBL.DeleteDeviceAuth(login, password, devUid);
        }



        //// оставлено для совместимости с мобильным клиентом
        //[HttpPost("/token")]
        //public async Task Token()
        //{
        //    var username = Request.Form["username"];
        //    var password = Request.Form["password"];

        //    var identity = GetIdentity(username, password);
        //    if (identity == null)
        //    {
        //        Response.StatusCode = 400;
        //        await Response.WriteAsync("Invalid login or password.");
        //        return;
        //    }

        //    var jwtConfig = _config.GetSection("JwtToken");
        //    string key = DataProtect.TryUnProtect(jwtConfig.GetValue("Key", AuthOptions.KEY));
        //    string issuer = jwtConfig.GetValue("Issuer", AuthOptions.ISSUER);
        //    string audience = jwtConfig.GetValue("Audience", AuthOptions.AUDIENCE);
        //    int lifetime = jwtConfig.GetValue("Lifetime", AuthOptions.LIFETIME);
        //    var now = DateTime.UtcNow;

        //    var jwt = new JwtSecurityToken(
        //            issuer: issuer,
        //            audience: audience,
        //            notBefore: now,
        //            claims: identity.Claims,
        //            expires: now.Add(TimeSpan.FromMinutes(lifetime)),
        //            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));
        //    var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

        //    var response = new
        //    {
        //        access_token = encodedJwt,
        //        username = identity.Name
        //    };

        //    Response.ContentType = "application/json";
        //    await Response.WriteAsync(JsonConvert.SerializeObject(response,
        //        new JsonSerializerSettings { Formatting = Formatting.Indented }));
        //}

        //private ClaimsIdentity GetIdentity(string login, string password)
        //{
        //    string hash = _userBL.CalcHash(password);
        //    User user = _userBL.GetUsers().FirstOrDefault(r =>
        //        r.Login == login && r.PasswordHash == hash);
        //    if (user == null) return null;

        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimsIdentity.DefaultNameClaimType, user.Login),
        //        new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
        //    };

        //    ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, "Token",
        //        ClaimsIdentity.DefaultNameClaimType,
        //        ClaimsIdentity.DefaultRoleClaimType);

        //    return claimsIdentity;
        //}
    }
}