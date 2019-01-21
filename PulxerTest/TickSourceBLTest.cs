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
    public class TickSourceBLTest
    {
        [Fact]
        public void SaveGet_insert_getEqualTickSource()
        {
            IInstrumBL instrumBL = new InstrumBLMock();
            ITickSourceDA tickSourceDA = new TickSourceDAMock();
            TickSourceBL tickSourceBL = new TickSourceBL(instrumBL, tickSourceDA, null, null);

            var ts0 = new TickSource(instrumBL, null, null);
            ts0.Name = "наименование";
            ts0.StartDate = new DateTime(2010, 1, 1);
            ts0.EndDate = new DateTime(2010, 12, 31);
            ts0.Timeframe = Timeframes.Hour;
            ts0.AddInstrum(1);
            ts0.AddInstrum(2);
            ts0.AddInstrum(3);
            tickSourceBL.SaveTickSource(ts0);

            var ts = tickSourceBL.GetTickSourceByID(ts0.TickSourceID);

            Assert.Equal(ts0.TickSourceID, ts.TickSourceID);
            Assert.Equal(ts0.Name, ts.Name);
            Assert.Equal(ts0.StartDate, ts.StartDate);
            Assert.Equal(ts0.EndDate, ts.EndDate);
            Assert.Equal(ts0.Timeframe, ts.Timeframe);

            string ids0 = string.Join(',', ts0.GetInstrums().Select(r => r.InsID.ToString()).OrderBy(r => r));
            string ids = string.Join(',', ts.GetInstrums().Select(r => r.InsID.ToString()).OrderBy(r => r));
            Assert.Equal(ids0, ids);
        }

        [Fact]
        public void SaveGet_update_getChanged()
        {
            IInstrumBL instrumBL = new InstrumBLMock();
            ITickSourceDA tickSourceDA = new TickSourceDAMock();
            TickSourceBL tickSourceBL = new TickSourceBL(instrumBL, tickSourceDA, null, null);

            var ts0 = new TickSource(instrumBL, null, null);
            ts0.Name = "наименование";
            ts0.StartDate = new DateTime(2010, 1, 1);
            ts0.EndDate = new DateTime(2010, 12, 31);
            ts0.Timeframe = Timeframes.Hour;
            ts0.AddInstrum(1);
            ts0.AddInstrum(2);
            ts0.AddInstrum(3);
            tickSourceBL.SaveTickSource(ts0); // insert
            int id = ts0.TickSourceID;

            ts0.Name = "новое наименование";
            ts0.StartDate = new DateTime(2011, 1, 1);
            ts0.EndDate = new DateTime(2011, 12, 31);
            ts0.Timeframe = Timeframes.Min10;
            ts0.AddInstrum(4);
            ts0.RemoveInstrum(1);
            tickSourceBL.SaveTickSource(ts0); // update

            var ts = tickSourceBL.GetTickSourceByID(id);

            Assert.Equal(id, ts.TickSourceID);
            Assert.Equal(ts0.Name, ts.Name);
            Assert.Equal(ts0.StartDate, ts.StartDate);
            Assert.Equal(ts0.EndDate, ts.EndDate);
            Assert.Equal(ts0.Timeframe, ts.Timeframe);

            string ids0 = string.Join(',', ts0.GetInstrums().Select(r => r.InsID.ToString()).OrderBy(r => r));
            string ids = string.Join(',', ts.GetInstrums().Select(r => r.InsID.ToString()).OrderBy(r => r));
            Assert.Equal(ids0, ids);
        }

        [Fact]
        public void GetList_create_getList()
        {
            IInstrumBL instrumBL = new InstrumBLMock();
            ITickSourceDA tickSourceDA = new TickSourceDAMock();
            ITickSourceBL tickSourceBL = new TickSourceBL(instrumBL, tickSourceDA, null, null);

            var ts = new TickSource(instrumBL, null, null);
            ts.Name = "наименование";
            ts.StartDate = new DateTime(2010, 1, 1);
            ts.EndDate = new DateTime(2010, 12, 31);
            ts.Timeframe = Timeframes.Hour;
            ts.AddInstrum(1);
            ts.AddInstrum(2);
            ts.AddInstrum(3);
            tickSourceBL.SaveTickSource(ts); // insert

            var list = tickSourceBL.GetTickSourceList().ToList();
            Assert.Single(list);

            Assert.Equal(ts.TickSourceID, list[0].TickSourceID);
            Assert.Equal(ts.Name, list[0].Name);
        }

        [Fact]
        public void Delete_createAndDelete_emptyList()
        {
            IInstrumBL instrumBL = new InstrumBLMock();
            ITickSourceDA tickSourceDA = new TickSourceDAMock();
            ITickSourceBL tickSourceBL = new TickSourceBL(instrumBL, tickSourceDA, null, null);

            var ts = new TickSource(instrumBL, null, null);
            ts.Name = "наименование";
            ts.StartDate = new DateTime(2010, 1, 1);
            ts.EndDate = new DateTime(2010, 12, 31);
            ts.Timeframe = Timeframes.Hour;
            ts.AddInstrum(1);
            ts.AddInstrum(2);
            ts.AddInstrum(3);
            tickSourceBL.SaveTickSource(ts); // insert

            var list = tickSourceBL.GetTickSourceList().ToList();
            Assert.Single(list);

            tickSourceBL.DeleteTickSourceByID(ts.TickSourceID);

            var list1 = tickSourceBL.GetTickSourceList().ToList();
            Assert.Empty(list1);
        }
    }
}
