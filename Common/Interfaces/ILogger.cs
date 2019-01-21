using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Interfaces
{
    public interface ILogger
    {
        void AddInfo(string source, string msg);
        void AddException(string source, Exception ex);
    }
}
