using Common;
using Microsoft.EntityFrameworkCore;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Storage.Test
{
    public class InsStorePeriodsTest
    {
        private readonly DbContextOptions<DaContext> _options;
        private readonly InsStoreDA _insStoreDA;
        private readonly InstrumDA _insDA;

        public InsStorePeriodsTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

            _insStoreDA = new InsStoreDA(_options);
            _insDA = new InstrumDA(_options);
        }

        [Fact]
        public void UpdatePeriods_()
        {
            int insID = _insDA.InsertInstrum("INS", "", "", 10, 2, 1);

            var s = _insStoreDA.GetInsStore(insID, Timeframes.Min);
            if (s != null)
            {
                _insStoreDA.DeleteInsStoreByID(s.InsStoreID);
            }
            int id = _insStoreDA.CreateInsStore(insID, Timeframes.Min, true);

            List<InsStorePeriod> periods = new List<InsStorePeriod>();
            periods.Add(new InsStorePeriod(new DateTime(2018, 1, 1), new DateTime(2018, 1, 1), false));
            _insStoreDA.UpdatePeriods(id, periods);
            var periods1 = _insStoreDA.GetPeriods(id).ToList();

            Assert.Equal(new DateTime(2018, 1, 1), periods1.First().StartDate);
            Assert.Equal(new DateTime(2018, 1, 1), periods1.First().EndDate);
            Assert.False(periods1.First().IsLastDirty);

            periods.Add(new InsStorePeriod(new DateTime(2018, 1, 2), new DateTime(2018, 1, 3), true));
            _insStoreDA.UpdatePeriods(id, periods);
            periods1 = _insStoreDA.GetPeriods(id).ToList();

            Assert.Equal(2, periods1.Count);

            periods.RemoveAt(0);
            _insStoreDA.UpdatePeriods(id, periods);
            periods1 = _insStoreDA.GetPeriods(id).ToList();

            Assert.Single(periods1);

            // cleanup
            periods1.Clear();
            _insStoreDA.UpdatePeriods(id, periods1);

            periods1 = _insStoreDA.GetPeriods(id).ToList();

            Assert.Empty(periods1);
        }

        [Fact]
        public void FreeDays_()
        {
            int insID = _insDA.InsertInstrum("INS", "", "", 10, 2, 1);

            var s = _insStoreDA.GetInsStore(insID, Timeframes.Min);
            if (s != null)
            {
                _insStoreDA.DeleteInsStoreByID(s.InsStoreID);
            }
            int id = _insStoreDA.CreateInsStore(insID, Timeframes.Min, true);

            List<DateTime> freeDays = new List<DateTime>();
            freeDays.Add(new DateTime(2018, 1, 1));
            _insStoreDA.UpdateFreeDays(id, freeDays);
            var freeDays1 = _insStoreDA.GetFreeDays(id).ToList();

            Assert.Equal(new DateTime(2018, 1, 1), freeDays1.First().Date);

            freeDays.Add(new DateTime(2018, 1, 2));
            _insStoreDA.UpdateFreeDays(id, freeDays);
            freeDays1 = _insStoreDA.GetFreeDays(id).ToList();

            Assert.Equal(2, freeDays1.Count);

            freeDays.RemoveAt(0);
            _insStoreDA.UpdateFreeDays(id, freeDays);
            freeDays1 = _insStoreDA.GetFreeDays(id).ToList();

            Assert.Single(freeDays1);

            // cleanup
            freeDays1.Clear();
            _insStoreDA.UpdateFreeDays(id, freeDays1);
            freeDays1 = _insStoreDA.GetFreeDays(id).ToList();

            Assert.Empty(freeDays1);
        }
    }
}
