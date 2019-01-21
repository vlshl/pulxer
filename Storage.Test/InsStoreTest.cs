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
    public class InsStoreTest
    {
        private readonly DbContextOptions<DaContext> _options;
        private readonly InsStoreDA _insStoreDA;
        private readonly InstrumDA _insDA;

        public InsStoreTest()
        {
            DbContextOptionsBuilder<DaContext> builder = new DbContextOptionsBuilder<DaContext>();
            builder.UseNpgsql("Username=postgres;Password=123;Host=localhost;Port=5432;Database=pulxer_test");
            _options = builder.Options;

            _insStoreDA = new InsStoreDA(_options);
            _insDA = new InstrumDA(_options);
        }

        [Fact]
        public void CreateGetDelete_()
        {
            int ins1ID = _insDA.InsertInstrum("INS1", "", "", 10, 2, 1);
            int ins2ID = _insDA.InsertInstrum("INS2", "", "", 1, 0, 0);

            int id1 = _insStoreDA.CreateInsStore(ins1ID, Timeframes.Min, true);
            int id2 = _insStoreDA.CreateInsStore(ins2ID, Timeframes.Hour, true);
            int id3 = _insStoreDA.CreateInsStore(ins1ID, Timeframes.Min5, false);

            var insStore1 = _insStoreDA.GetInsStoreByID(id1);
            var insStore2 = _insStoreDA.GetInsStore(ins2ID, Timeframes.Hour);
            var actives = _insStoreDA.GetInsStores(null, null, true);
            var gazpInsStores = _insStoreDA.GetInsStores(ins1ID, null, true);

            Assert.Equal(id1, insStore1.InsStoreID);
            Assert.Equal(id2, insStore2.InsStoreID);
            Assert.Contains(id1, actives.Select(r => r.InsStoreID));
            Assert.Contains(id2, actives.Select(r => r.InsStoreID));
            Assert.DoesNotContain(id3, actives.Select(r => r.InsStoreID));
            Assert.Contains(id1, gazpInsStores.Select(r => r.InsStoreID));
            Assert.DoesNotContain(id3, gazpInsStores.Select(r => r.InsStoreID));

            // cleanup
            _insStoreDA.DeleteInsStoreByID(id1);
            _insStoreDA.DeleteInsStoreByID(id2);
            _insStoreDA.DeleteInsStoreByID(id3);
        }

        [Fact]
        public void Update_()
        {
            int ins1ID = _insDA.InsertInstrum("INS1", "", "", 10, 2, 1);

            int id = _insStoreDA.CreateInsStore(ins1ID, Timeframes.Min, true);

            var insStore = _insStoreDA.GetInsStoreByID(id);

            Assert.Equal(id, insStore.InsStoreID);
            Assert.Equal(ins1ID, insStore.InsID);
            Assert.Equal(Timeframes.Min, insStore.Tf);
            Assert.True(insStore.IsEnable);

            _insStoreDA.UpdateInsStore(id, false);
            insStore = _insStoreDA.GetInsStoreByID(id);

            Assert.Equal(id, insStore.InsStoreID);
            Assert.Equal(ins1ID, insStore.InsID);
            Assert.Equal(Timeframes.Min, insStore.Tf);
            Assert.False(insStore.IsEnable);

            _insStoreDA.UpdateInsStore(id, true);
            insStore = _insStoreDA.GetInsStoreByID(id);

            Assert.Equal(id, insStore.InsStoreID);
            Assert.Equal(ins1ID, insStore.InsID);
            Assert.Equal(Timeframes.Min, insStore.Tf);
            Assert.True(insStore.IsEnable);

            //cleanup
            _insStoreDA.DeleteInsStoreByID(id);
        }

        [Fact]
        public void InsertBars()
        {
            List<Bar> bars = new List<Bar>();
            DateTime time = new DateTime(2018, 1, 1, 10, 0, 0);
            for (int i = 0; i < 480; i++)
            {
                Bar b = new Bar(time, Timeframes.Min);
                b.Open = 100; b.High = 110; b.Low = 95; b.Close = 105; b.Volume = 1000;
                bars.Add(b);
                time = time.AddMinutes(1);
            }
            time = new DateTime(2018, 1, 2, 10, 0, 0);
            for (int i = 0; i < 480; i++)
            {
                Bar b = new Bar(time, Timeframes.Min);
                b.Open = 101; b.High = 121; b.Low = 93; b.Close = 115; b.Volume = 100;
                bars.Add(b);
                time = time.AddMinutes(1);
            }
            time = new DateTime(2018, 1, 3, 10, 0, 0);
            for (int i = 0; i < 480; i++)
            {
                Bar b = new Bar(time, Timeframes.Min);
                b.Open = 101; b.High = 118; b.Low = 87; b.Close = 109; b.Volume = 537;
                bars.Add(b);
                time = time.AddMinutes(1);
            }

            int k = (int)Math.Pow(10, 2);

            var dbBars = bars.Select(b =>
            {
                DbBarHistory bh = new DbBarHistory();
                bh.InsStoreID = 1;
                bh.Time = StorageLib.ToDbTime(b.Time);

                int op = (int)(b.Open * k);
                int cp = (int)(b.Close * k);
                int hp = (int)(b.High * k);
                int lp = (int)(b.Low * k);

                bh.OpenPrice = op;
                bh.CloseDelta = CalcDelta(cp, op);
                bh.HighDelta = CalcDelta(hp, op);
                bh.LowDelta = CalcDelta(lp, op);

                long v = b.Volume;
                if (v > int.MaxValue) v = int.MaxValue;
                if (v < int.MinValue) v = int.MinValue;
                bh.Volume = (int)v;

                return bh;
            });

            int ins1ID = _insDA.InsertInstrum("INS1", "", "", 10, 2, 1);

            var insStore = _insStoreDA.GetInsStore(ins1ID, Timeframes.Min);
            if (insStore != null)
            {
                _insStoreDA.DeleteInsStoreByID(insStore.InsStoreID);
            }
            int insStoreID = _insStoreDA.CreateInsStore(ins1ID, Timeframes.Min, true);

            _insStoreDA.InsertBars(insStoreID, dbBars,
                new DateTime(2018, 1, 1), new DateTime(2018, 1, 3),
                new CancellationToken());

            var bars1 = _insStoreDA.GetHistoryAsync(insStoreID, new DateTime(2018, 1, 1), new DateTime(2018, 1, 3)).Result;

            Assert.Equal(1440, bars1.Count());

            _insStoreDA.DeleteBars(insStoreID, new DateTime(2018, 1, 1), new DateTime(2018, 1, 1));
            var bars2 = _insStoreDA.GetHistoryAsync(insStoreID, new DateTime(2018, 1, 1), new DateTime(2018, 1, 3)).Result;

            Assert.Equal(960, bars2.Count());

            _insStoreDA.DeleteBars(insStoreID, new DateTime(2018, 1, 2), new DateTime(2018, 1, 3));
            var bars3 = _insStoreDA.GetHistoryAsync(insStoreID, new DateTime(2018, 1, 1), new DateTime(2018, 1, 3)).Result;

            Assert.Empty(bars3);

            //cleanup
            _insStoreDA.DeleteInsStoreByID(insStoreID);
        }

        private static short CalcDelta(int p1, int p)
        {
            int d = p1 - p;
            if (d > short.MaxValue) d = short.MaxValue;
            if (d < short.MinValue) d = short.MinValue;

            return (short)d;
        }

    }
}
