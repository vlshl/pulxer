using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cli
{
    public class AccountCtrl
    {
        private readonly IConsole _console;
        private readonly IAccountBL _accountBL;

        public AccountCtrl(IConsole console, IAccountBL accountBL)
        {
            _console = console;
            _accountBL = accountBL;
        }

        public void ListAccounts()
        {
            try
            {
                var list = _accountBL.GetAccountList();

                _console.WriteLine("Id\tCode\tName");
                _console.WriteSeparator();

                foreach (var acc in list)
                {
                    string str = string.Format("{0}\t{1}\t{2}",
                        acc.AccountID.ToString(),
                        acc.Code,
                        acc.Name);
                    _console.WriteLine(str);
                }
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        public void DeleteAccount(List<string> args)
        {
            if (args.Count < 1)
            {
                _console.WriteError("Неверное число аргументов.");
                return;
            }

            int accountID;
            int r;
            if (int.TryParse(args[0].Trim(), out r))
            {
                accountID = r;
            }
            else
            {
                _console.WriteError("Неверно указан id счета");
                return;
            }

            bool isFullDelete = false;
            if (args.Count >= 2 && args[1] == "full")
            {
                isFullDelete = true;
            }

            try
            {
                bool isSuccess = _accountBL.DeleteTestAccountData(accountID, isFullDelete);
                if (!isSuccess)
                {
                    _console.WriteError("Счет не найден или не является тестовым.");
                    return;
                }
                _console.WriteLine(isFullDelete ? "Счет полностью удален вместе со всеми данными." : "Данные по счету удалены, сам счет не был удален.");
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }
    }
}
