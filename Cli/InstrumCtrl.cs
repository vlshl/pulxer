using Common.Interfaces;
using System.Linq;

namespace Cli
{
    public class InstrumCtrl
    {
        private readonly IInstrumDA _instrumDA;
        private readonly IConsole _console;

        public InstrumCtrl(IConsole console, IInstrumDA instrumDA)
        {
            _console = console;
            _instrumDA = instrumDA;
        }

        public void ListInstrum()
        {
            var instrums = _instrumDA.GetInstrumList();

            _console.WriteLine("InsID\tTicker\tShortName");
            _console.WriteSeparator();
            foreach (var instrum in instrums)
            {
                _console.WriteLine(string.Format("{0}\t{1}\t{2}",
                    instrum.InsID.ToString(),
                    instrum.Ticker,
                    instrum.ShortName));
            }
            _console.WriteLine(string.Format("Instrums: {0}", instrums.Count().ToString()));
        }
    }
}
