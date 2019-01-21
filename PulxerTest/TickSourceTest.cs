using Common;
using Common.Data;
using Common.Interfaces;
using Platform;
using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PulxerTest
{
    public class TickSourceTest
    {
        [Fact]
        public void SerializeInitialize_tsclone_equalCloned()
        {
            IInstrumBL instrumBL = new InstrumBLMock();
            TickSource ts = new TickSource(instrumBL, null, null);
            ts.Name = "наименование";
            ts.StartDate = new DateTime(2010, 1, 1);
            ts.EndDate = new DateTime(2010, 12, 31);
            ts.Timeframe = Timeframes.Min;
            ts.AddInstrum(1);
            ts.AddInstrum(2);
            ts.AddInstrum(3);

            var xdoc = ts.Serialize();
            var ts1 = new TickSource(instrumBL, null, null);
            ts1.Initialize(xdoc);

            Assert.Equal(ts.StartDate, ts1.StartDate);
            Assert.Equal(ts.EndDate, ts1.EndDate);
            Assert.Equal(ts.Timeframe, ts1.Timeframe);
            var ts1_instrums = ts1.GetInstrums();
            Assert.Equal(3, ts1_instrums.Count());
            Assert.Contains(ts1_instrums, r => r.InsID == 1);
            Assert.Contains(ts1_instrums, r => r.InsID == 2);
            Assert.Contains(ts1_instrums, r => r.InsID == 3);
        }

        [Fact]
        public void SerializeInitialize_tsWithEmptyInstrums_clonedWithEmptyInstrums()
        {
            IInstrumBL instrumBL = new InstrumBLMock();
            TickSource ts = new TickSource(instrumBL, null, null);
            ts.Name = "наименование";
            ts.StartDate = new DateTime(2010, 1, 1);
            ts.EndDate = new DateTime(2010, 12, 31);
            ts.Timeframe = Timeframes.Min;

            var xdoc = ts.Serialize();
            var ts1 = new TickSource(instrumBL, null, null);
            ts1.Initialize(xdoc);

            var ts1_instrums = ts1.GetInstrums();
            Assert.Empty(ts1_instrums);
        }
    }
}
