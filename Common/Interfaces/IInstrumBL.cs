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
        void DeleteInstrumByID(int insID);
        int[] GetFavInstrumIds(int userId);
        int[] AddFavorite(int userId, int instrumId);
        int[] RemoveFavorite(int userId, int instrumId);
        int[] GetActiveInstrumIds();
    }
}





