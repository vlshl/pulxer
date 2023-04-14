using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public delegate void OnPxTick(DateTime time, int insId, int lots, decimal price);
    
    public interface IPluginPlatform
    {
        void AddLog(string source, string text);
        IInstrum GetInstrum(string ticker);
        Task<IBarRow> CreateBarRow(int insId, Timeframes tf, int historyDays);
        void Close();
        void Subscribe(int insId, OnPxTick onTick);
        DateTime GetSessionDate();
    }
}
