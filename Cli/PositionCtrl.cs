using Common.Data;
using Common.Interfaces;
using Pulxer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Cli
{
    public class PositionCtrl
    {
        private readonly IConsole _console;
        private readonly IInstrumBL _instrumBL;
        private readonly IAccountDA _accountDA;

        public PositionCtrl(IConsole console, IInstrumBL instrumBL, IAccountDA accountDA)
        {
            _console = console;
            _instrumBL = instrumBL;
            _accountDA = accountDA;
        }

        public void LoadTrades(List<string> args)
        {
            if (args.Count < 2)
            {
                _console.WriteError("Неверное число аргументов");
                return;
            }

            int accountID;
            if (!int.TryParse(args[0].Trim(), out accountID))
            {
                _console.WriteError("Неверно указан id счета");
                return;
            }

            if (!File.Exists(args[1]))
            {
                _console.WriteError("Файл не существует: " + args[1]);
                return;
            }

            var loader = new TradesLoader(_instrumBL, _accountDA);

            try
            {
                string log = loader.Load(accountID, args[1]);
                string logfile = args[1] + ".log";
                File.WriteAllText(logfile, log);
                _console.WriteLine("Сформирован log-файл: " + logfile);
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }
    }
}
