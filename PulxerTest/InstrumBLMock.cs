using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PulxerTest
{
    public class InstrumBLMock : IInstrumBL
    {
        private List<Instrum> _instrums;

        public InstrumBLMock()
        {
            _instrums = new List<Instrum>();

            Instrum instrum1 = new Instrum()
            {
                InsID = 1,
                Ticker = "ticker1",
                Name = "name1",
                ShortName = "shortname1",
                Decimals = 0,
                LotSize = 1,
                PriceStep = 0.01m
            };
            _instrums.Add(instrum1);

            Instrum instrum2 = new Instrum()
            {
                InsID = 2,
                Ticker = "ticker2",
                Name = "name2",
                ShortName = "shortname2",
                Decimals = 0,
                LotSize = 1,
                PriceStep = 0.01m
            };
            _instrums.Add(instrum2);

            Instrum instrum3 = new Instrum()
            {
                InsID = 3,
                Ticker = "ticker3",
                Name = "name3",
                ShortName = "shortname3",
                Decimals = 0,
                LotSize = 1,
                PriceStep = 0.01m
            };
            _instrums.Add(instrum3);
        }

        public void AddFavorite(int userId, int instrumId)
        {
            throw new NotImplementedException();
        }

        public void DeleteInstrumByID(int insID)
        {
            throw new NotImplementedException();
        }

        public int[] GetActiveInstrumIds()
        {
            throw new NotImplementedException();
        }

        public int[] GetFavInstrumIds(int userId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<InstrumListItem> GetFavInstrumList(int userId)
        {
            throw new NotImplementedException();
        }

        public Instrum GetInstrum(string ticker)
        {
            return _instrums.FirstOrDefault(r => r.Ticker == ticker);
        }

        public Instrum GetInstrumByID(int insID)
        {
            return _instrums.FirstOrDefault(r => r.InsID == insID);
        }

        public IEnumerable<InstrumListItem> GetInstrumList()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Instrum> GetInstrums()
        {
            return _instrums;
        }

        public void RemoveFavorite(int userId, int instrumId)
        {
            throw new NotImplementedException();
        }

        public void SaveInstrum(Instrum ins)
        {
            throw new NotImplementedException();
        }

        int[] IInstrumBL.AddFavorite(int userId, int instrumId)
        {
            throw new NotImplementedException();
        }

        int[] IInstrumBL.RemoveFavorite(int userId, int instrumId)
        {
            throw new NotImplementedException();
        }
    }
}
