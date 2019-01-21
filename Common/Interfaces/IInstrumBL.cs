using System;
using System.Collections.Generic;
using System.Text;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// Instrums subsystem interface
    /// </summary>
    public interface IInstrumBL
    {
        IEnumerable<CommonData.InstrumListItem> GetInstrumList();
        IEnumerable<CommonData.Instrum> GetInstrums();
        CommonData.Instrum GetInstrumByID(int insID);
        CommonData.Instrum GetInstrum(string ticker);
        void SaveInstrum(CommonData.Instrum ins);
        void DeleteInstrumByID(int insID);
    }
}





