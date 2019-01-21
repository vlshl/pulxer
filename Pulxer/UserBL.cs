using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pulxer
{
    public class UserBL : IUserBL
    {
        private readonly IUserDA _userDA;

        public UserBL(IUserDA userDA)
        {
            _userDA = userDA;
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
        /// Удаление пользователя по логину
        /// </summary>
        /// <param name="login">Логин</param>
        /// <returns>true-пользователь успешно удален, false-пользователь с таким логином не существует</returns>
        public bool DeleteUser(string login)
        {
            User user = _userDA.GetUsers().FirstOrDefault(r => r.Login == login);
            if (user == null) return false;

            _userDA.DeleteUser(user.UserID);

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
    }
}
