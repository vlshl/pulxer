using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
    public class UserCtrl
    {
        private readonly IConsole _console;
        private readonly IUserBL _userBL;

        public UserCtrl(IConsole console, IUserBL userBL)
        {
            _console = console;
            _userBL = userBL;
        }

        public void ListUsers()
        {
            try
            {
                var users = _userBL.GetUsers();

                _console.WriteLine("Id\tLogin\tPassword Hash\t\t\tRole");
                _console.WriteSeparator();

                foreach (var user in users)
                {
                    string str = string.Format("{0}\t{1}\t{2}\t{3}",
                        user.UserID.ToString(),
                        user.Login,
                        user.PasswordHash,
                        user.Role);
                    _console.WriteLine(str);
                }
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        public void CreateUser(List<string> args)
        {
            if (args.Count < 3)
            {
                _console.WriteError("Неверное число аргументов.");
                return;
            }

            try
            {
                var user = _userBL.CreateUser(args[0], args[1], args[2]);
                if (user == null)
                {
                    _console.WriteError("Логин уже существует.");
                    return;
                }
                _console.WriteLine("Пользователь создан, Id=" + user.UserID.ToString());
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        public void SetPassword(List<string> args)
        {
            if (args.Count < 2)
            {
                _console.WriteError("Неверное число аргументов.");
                return;
            }

            try
            {
                bool isSuccess = _userBL.SetPassword(args[0], args[1]);
                if (!isSuccess)
                {
                    _console.WriteError("Пользователь не найден.");
                    return;
                }
                _console.WriteLine("Пароль изменен.");
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        public void DeleteUser(List<string> args)
        {
            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов.");
                return;
            }

            try
            {
                bool isSuccess = _userBL.DeleteUser(args[0]);
                if (!isSuccess)
                {
                    _console.WriteError("Пользователь не найден.");
                    return;
                }
                _console.WriteLine("Пользователь удален.");
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }
    }
}
