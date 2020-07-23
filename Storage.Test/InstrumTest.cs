using Common.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Storage.Test
{
    public class InstrumTest
    {
        private readonly DbContextOptions<DaContext> _options;
        private readonly InstrumDA _instrumDA;

        public InstrumTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;

            _instrumDA = new InstrumDA(_options);
        }

        [Fact]
        public void InsertGetInstrums_()
        {
            int id1 = _instrumDA.InsertInstrum("TICKER1", "Тикер1", "Тикер 1", 10, 2, 0.1m);
            int id2 = _instrumDA.InsertInstrum("TICKER2", "Тикер2", "Тикер 2", 1, 3, 1);

            var list = _instrumDA.GetInstrumList();
            var instrums = _instrumDA.GetInstrums();

            var item = list.FirstOrDefault(r => r.InsID == id1);
            var ins = instrums.FirstOrDefault(r => r.InsID == id1);

            Assert.Equal(id1, item.InsID);
            Assert.Equal("TICKER1", item.Ticker);
            Assert.Equal("Тикер1", item.ShortName);

            Assert.Equal(id1, ins.InsID);
            Assert.Equal("TICKER1", ins.Ticker);
            Assert.Equal("Тикер1", ins.ShortName);
            Assert.Equal("Тикер 1", ins.Name);
            Assert.Equal(2, ins.Decimals);
            Assert.Equal(10, ins.LotSize);
            Assert.Equal(0.1m, ins.PriceStep);

            var ins1 = _instrumDA.GetInstrum(id1);
            var ins2 = _instrumDA.GetInstrum(0, "TICKER2");

            Assert.Equal(id1, ins1.InsID);
            Assert.Equal("TICKER1", ins1.Ticker);
            Assert.Equal("Тикер1", ins1.ShortName);
            Assert.Equal("Тикер 1", ins1.Name);
            Assert.Equal(2, ins1.Decimals);
            Assert.Equal(10, ins1.LotSize);
            Assert.Equal(0.1m, ins1.PriceStep);

            Assert.Equal(id2, ins2.InsID);
            Assert.Equal("TICKER2", ins2.Ticker);
            Assert.Equal("Тикер2", ins2.ShortName);
            Assert.Equal("Тикер 2", ins2.Name);
            Assert.Equal(3, ins2.Decimals);
            Assert.Equal(1, ins2.LotSize);
            Assert.Equal(1, ins2.PriceStep);

            _instrumDA.DeleteInstrumByID(id1);
            _instrumDA.DeleteInstrumByID(id2);
        }

        [Fact]
        public void UpdateDeleteInstrum_()
        {
            int id = _instrumDA.InsertInstrum("TICKER1", "Тикер1", "Тикер 1", 10, 2, 0.1m);
            _instrumDA.UpdateInstrum(id, "t", "sn", "name", 20, 100, 0.001m);
            var ins1 = _instrumDA.GetInstrum(id);

            Assert.Equal("t", ins1.Ticker);
            Assert.Equal("sn", ins1.ShortName);
            Assert.Equal("name", ins1.Name);
            Assert.Equal(100, ins1.Decimals);
            Assert.Equal(20, ins1.LotSize);
            Assert.Equal(0.001m, ins1.PriceStep);

            _instrumDA.DeleteInstrumByID(id);
            var ins_null = _instrumDA.GetInstrum(id);
            Assert.Null(ins_null);
        }
    }
}
