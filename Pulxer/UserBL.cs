using Common;
using Common.Data;
using Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Pulxer
{
    public class UserBL : IUserBL
    {
        private readonly IUserDA _userDA;
        private readonly IDeviceDA _deviceDA;

        public UserBL(IUserDA userDA, IDeviceDA deviceDA)
        {
            _userDA = userDA;
            _deviceDA = deviceDA;
        }

        /// <summary>
        /// Создание нового пользователя
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <param name="role">Роль</param>
        /// <returns>Объект пользователь или null (null - логин уже существует)</returns>
        public User CreateUser(string login, string password, string role)
        {
            if (_userDA.GetUsers().Any(r => r.Login == login))
                return null;

            return _userDA.CreateUser(login, CalcHash(password), role);
        }

        /// <summary>
        /// Список пользователей
        /// </summary>
        /// <returns>Список</returns>
        public IEnumerable<User> GetUsers()
        {
            return _userDA.GetUsers();
        }

        /// <summary>
        /// Пользователь
        /// </summary>
        /// <param name="userId">Идентификатор пользователя</param>
        /// <returns>Пользователь или null</returns>
        public User GetUserById(int userId)
        {
            return _userDA.GetUsers().FirstOrDefault(u => u.UserId == userId);
        }

        /// <summary>
        /// Установить пароль для пользователя по логину
        /// </summary>
        /// <param name="login">Логин</param>
        /// <param name="password">Новый пароль</param>
        /// <returns>true-пароль успешно установлен, false-пользователь не существует</returns>
        public bool SetPassword(string login, string password)
        {
            User user = _userDA.GetUsers().FirstOrDefault(r => r.Login == login);
            if (user == null) return false;

            user.PasswordHash = CalcHash(password);
            _userDA.UpdateUser(user);

            return true;
        }

        /// <summary>
        /// Удаление пользователя по логину. Также удаляются все связанные с пользователем устройства.
        /// </summary>
        /// <param name="login">Логин</param>
        /// <returns>true-пользователь успешно удален, false-пользователь с таким логином не существует</returns>
        public bool DeleteUser(string login)
        {
            User user = _userDA.GetUsers().FirstOrDefault(r => r.Login == login);
            if (user == null) return false;

            _deviceDA.DeleteDevicesByUser(user.UserId);
            _userDA.DeleteUser(user.UserId);

            return true;
        }

        /// <summary>
        /// Вычисление хэша для строки.
        /// Строка преобразуется в набор байтов через UTF8,
        /// затем считается хэш MD5, 
        /// результат преобразуется в base64.
        /// </summary>
        /// <param name="text">Входная строка</param>
        /// <returns>Результат в base64</returns>
        public string CalcHash(string text)
        {
            var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(text);
            var hash = md5.ComputeHash(bytes);

            return Convert.ToBase64String(hash);
        }

        public User AuthUser(string login, string password)
        {
            if (string.IsNullOrEmpty(login)) return null;
            if (password == null) password = "";

            var user = _userDA.GetUsers().FirstOrDefault(r => r.Login == login);
            if (user == null || user.PasswordHash != CalcHash(password)) return null;

            return user;
        }

        public string BuildJwtToken(IConfiguration config, User user, out DateTime expTime)
        {
            var jwtConfig = config.GetSection("JwtToken");
            string key = DataProtect.TryUnProtect(jwtConfig.GetValue("Key", AuthOptions.KEY));
            string issuer = jwtConfig.GetValue("Issuer", AuthOptions.ISSUER);
            string audience = jwtConfig.GetValue("Audience", AuthOptions.AUDIENCE);
            int lifetime = jwtConfig.GetValue("Lifetime", AuthOptions.LIFETIME);
            var now = DateTime.UtcNow;
            expTime = now.Add(TimeSpan.FromMinutes(lifetime));

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, user.UserId.ToString()),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, user.Role)
            }; 

            var jwt = new JwtSecurityToken(
                    issuer: issuer,
                    audience: audience,
                    notBefore: now,
                    claims: claims,
                    expires: expTime,
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        /// <summary>
        /// Аутентификация устройства
        /// </summary>
        /// <param name="devUid">Уникальный код устройства</param>
        /// <param name="code">Пин-код</param>
        /// <returns>Device-устройство, null-аутентификация не прошла</returns>
        public Device AuthDevice(string devUid, string code)
        {
            if (string.IsNullOrEmpty(devUid) || string.IsNullOrEmpty(code)) return null;

            var dev = _deviceDA.GetDevice(devUid);
            if ((dev == null) || (dev.Code != code)) return null;

            return dev;
        }

        /// <summary>
        /// Создание аутентификации устройства. Создается связь между пользователем с указанным логином и устройством.
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль</param>
        /// <param name="code">Пин-код</param>
        /// <returns>Уникальный код устройства, пустая строка - неверный логин и пароль либо иные причины ошибки</returns>
        public string CreateDeviceAuth(string login, string password, string code)
        {
            var user = AuthUser(login, password);
            if (user == null) return "";

            string devUid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var dev = _deviceDA.CreateDevice(devUid, code, user.UserId);
            if (dev == null) return "";

            return devUid;
        }

        /// <summary>
        /// Удаление аутентификации устройства
        /// </summary>
        /// <param name="login">Логин пользователя</param>
        /// <param name="password">Пароль</param>
        /// <param name="devUid">Уникальный идентификатор устройства</param>
        /// <returns>true-успешно, false-либо неверный логин и пароль, либо не найдено устройство, либо нейденное устройство не соответствует пользователю, либо иные причины ошибки</returns>
        public bool DeleteDeviceAuth(string login, string password, string devUid)
        {
            var user = AuthUser(login, password);
            if (user == null) return false;

            var dev = _deviceDA.GetDevice(devUid);
            if (dev == null) return false;
            if (dev.UserId != user.UserId) return false;

            return _deviceDA.DeleteDevice(dev.Uid);
        }
    }
}
