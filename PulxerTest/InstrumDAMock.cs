using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PulxerTest
{
    public class InstrumDAMock : IInstrumDA
    {
        private Dictionary<int, Instrum> _id_instrum;
        private int nextID = 1001;

        public InstrumDAMock()
        {
            _id_instrum = new Dictionary<int, Instrum>();
        }


        public void DeleteInstrumByID(int insID)
        {
            throw new NotImplementedException();
        }

        public Instrum GetInstrum(int insID, string ticker = null)
        {
            if (insID > 0 && _id_instrum.ContainsKey(insID))
                return _id_instrum[insID];

            if (insID == 0 && ticker != null)
            {
                return _id_instrum.Values.FirstOrDefault(r => r.Ticker == ticker);
            }

            return null;
        }

        public IEnumerable<InstrumListItem> GetInstrumList()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Instrum> GetInstrums()
        {
            return _id_instrum.Values;
        }

        public int InsertInstrum(string ticker, string shortName, string name, int lotSize, int decimals, decimal priceStep)
        {
            Instrum instrum = new Instrum()
            {
                InsID = nextID++,
                Ticker = ticker,
                ShortName = shortName,
                Name = name,
                LotSize = lotSize,
                Decimals = decimals,
                PriceStep = priceStep
            };
            _id_instrum.Add(instrum.InsID, instrum);

            return instrum.InsID;
        }

        public void UpdateInstrum(int insID, string shortName, string name, int lotSize, int decimals, decimal priceStep)
        {
            if (!_id_instrum.ContainsKey(insID)) return;

            var instrum = _id_instrum[insID];
            instrum.ShortName = shortName;
            instrum.Name = name;
            instrum.LotSize = lotSize;
            instrum.Decimals = decimals;
            instrum.PriceStep = priceStep;
        }
    }
}
