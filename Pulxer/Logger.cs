using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulxer
{
    public class Logger : ILogger
    {
        private string _logPath = "";
        private object _lock = new object();
        private List<string> _log = null;

        public Logger()
        {
            _log = new List<string>();
        }

        public void AddInfo(string source, string msg)
        {
            string text = string.Format("{0} {1}:{2} {3}", DateTime.Now.ToString("HH:mm:ss.fff"), "INFO", source, msg);
            lock (_log)
            {
                _log.Add(text);
            }
        }

        public void AddException(string source, Exception ex)
        {
            string text = string.Format("{0} {1}:{2} {3}", DateTime.Now.ToString("HH:mm:ss.fff"), "EXP", source, ex.ToString());
            lock (_log)
            {
                _log.Add(text);
            }
        }

        public IEnumerable<string> GetLogs()
        {
            lock (_log)
            {
                return _log.ToList();
            }
        }
    }
}
