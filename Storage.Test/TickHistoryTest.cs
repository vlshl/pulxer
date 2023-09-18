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
    public class TickHistoryTest
    {
        private readonly DbContextOptions<DaContext> _options;
        private readonly TickHistoryDA _tickHistoryDA;
        private readonly InstrumDA _insDA;
        private int _insID;

        public TickHistoryTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            _tickHistoryDA = new TickHistoryDA(_options);
            _insDA = new InstrumDA(_options);

            _insID = _insDA.InsertInstrum("III", "", "", 1, 1, 1);
        }

        [Fact]
        public void TickHistory_()
        {
            var instrums = _insDA.GetInstrums();
            byte[] data = new byte[1024];
            new Random().NextBytes(data);

            _tickHistoryDA.InsertData(_insID, new DateTime(2010, 1, 1), data);

            var dates = _tickHistoryDA.GetDates(_insID).ToList();

            Assert.Equal(new DateTime(2010, 1, 1), dates[0]);

            var dates_ = _tickHistoryDA.GetDates(null).ToList();

            Assert.Equal(new DateTime(2010, 1, 1), dates_[0]);

            var ins = _tickHistoryDA.GetInstrums(null).ToList();

            Assert.Equal(_insID, ins[0]);

            var ins_ = _tickHistoryDA.GetInstrums(new DateTime(2010, 1, 1)).ToList();

            Assert.Equal(_insID, ins_[0]);

            var data1 = _tickHistoryDA.GetData(_insID, new DateTime(2010, 1, 1));
            for (int i = 0; i < data.Length; i++)
            {
                Assert.Equal(data[i], data1[i]);
            }

            _tickHistoryDA.DeleteData(_insID, new DateTime(2010, 1, 2)); // не удалено, т.к. дата не совпадает
            var dates1 = _tickHistoryDA.GetDates(_insID).ToList();

            _tickHistoryDA.DeleteData(_insID + 1, new DateTime(2010, 1, 1)); // не удалено, т.к. инструмент не тот
            var dates2 = _tickHistoryDA.GetDates(_insID).ToList();

            _tickHistoryDA.DeleteData(_insID, new DateTime(2010, 1, 1)); // удалено
            var dates3 = _tickHistoryDA.GetDates(_insID).ToList();

            Assert.True(dates1.Any());
            Assert.True(dates2.Any());
            Assert.False(dates3.Any());
        }

    }
}
