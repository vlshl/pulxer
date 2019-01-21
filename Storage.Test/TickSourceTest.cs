using Common.Data;
using Microsoft.EntityFrameworkCore;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Storage.Test
{
    public class TickSourceTest
    {
        private readonly DbContextOptions<DaContext> _options;
        private readonly TickSourceDA _tickSourceDA;

        public TickSourceTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;

            _tickSourceDA = new TickSourceDA(_options);
        }

        [Fact]
        public void TickSource_()
        {
            // создание TickSource
            DbTickSource ts = new DbTickSource();
            ts.Name = "name"; ts.DataStr = "datastr";
            int tsID = _tickSourceDA.InsertTickSource(ts);

            // вывод списка TickSource
            var list = _tickSourceDA.GetTickSources().ToList();

            Assert.Single(list.Where(r => r.TickSourceID == tsID));

            // вывод tickSource
            var tickSource = _tickSourceDA.GetTickSourceByID(list.ElementAt(0).TickSourceID);

            Assert.Equal(tsID, tickSource.TickSourceID);
            Assert.Equal("name", tickSource.Name);
            Assert.Equal("datastr", tickSource.DataStr);

            // изменение
            tickSource.Name = "name1"; tickSource.DataStr = "datastr1";
            _tickSourceDA.UpdateTickSource(tickSource);

            // вывод измененного tickSource
            var tickSource1 = _tickSourceDA.GetTickSourceByID(tickSource.TickSourceID);

            Assert.Equal(tsID, tickSource1.TickSourceID);
            Assert.Equal("name1", tickSource1.Name);
            Assert.Equal("datastr1", tickSource1.DataStr);

            // удаление
            _tickSourceDA.DeleteTickSourceByID(tickSource.TickSourceID);

            // должен быть пустой список
            var list1 = _tickSourceDA.GetTickSources();

            Assert.Empty(list1);
        }

    }
}
