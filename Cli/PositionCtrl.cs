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
        private readonly IPositionBL _positionBL;
        private readonly IAccountDA _accountDA;

        public PositionCtrl(IConsole console, IInstrumBL instrumBL, IPositionBL positionBL, IAccountDA accountDA)
        {
            _console = console;
            _instrumBL = instrumBL;
            _positionBL = positionBL;
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

        public void RefreshPositions(List<string> args)
        {
            if (args.Count < 1)
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

            try
            {
                _positionBL.RefreshPositions(accountID);
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        public void GetPositions(List<string> args, bool isOpenOnly)
        {
            if (args.Count < 1)
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

            try
            {
                IEnumerable<Position> posList = null;
                if (isOpenOnly)
                {
                    posList = _positionBL.GetOpenPositions(accountID);
                }
                else
                {
                    posList = _positionBL.GetAllPositions(accountID);
                }

                foreach (var pos in posList)
                {
                    PrintPosition(pos, _instrumBL);
                }
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        public void ClearPositions(List<string> args)
        {
            if (args.Count < 1)
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

            try
            {
                _positionBL.ClearPositions(accountID);
            }
            catch (Exception ex)
            {
                _console.WriteError(ex.ToString());
            }
        }

        private void PrintPosition(Position pos, IInstrumBL instrumBL)
        {
            var instrum = instrumBL.GetInstrumByID(pos.InsID);
            if (instrum == null) return;
            _console.WriteLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                instrum.Ticker,
                pos.OpenTime.ToString("dd.MM.yyyy HH:mm:ss"),
                pos.OpenPrice.ToString(),
                pos.Count.ToString(),
                pos.PosType.ToString(),
                pos.CloseTime != null ? pos.CloseTime.Value.ToString("dd.MM.yyyy HH:mm:ss") : "",
                pos.ClosePrice != null ? pos.ClosePrice.Value.ToString() : ""));
        }
    }
}
