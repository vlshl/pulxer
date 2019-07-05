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
    public class SeriesDataTest
    {
        [Fact]
        public void Series_fulltest()
        {
            IAccountDA accDA = new AccountDAMock();
            SeriesData sd = new SeriesData(accDA);
            int accID = 10;

            // создали серию
            int sid = sd.OpenSeries("key1");

            // записали туда числа
            DateTime d = new DateTime(2019, 1, 1);
            sd.AddSeriesValue(sid, d, 100);

            // сохранили
            sd.SetAccount(accID);
            sd.SaveData();

            // загрузили
            SeriesData sd1 = new SeriesData(accDA);
            sd1.LoadData(accID);

            // создали новую серию
            int sid1 = sd1.OpenSeries("key1"); // старая
            int sid2 = sd1.OpenSeries("key2"); // новая

            // записали числа в старую серию
            sd1.AddSeriesValue(sid1, d, 200);

            // записали числа в новую серию
            sd1.AddSeriesValue(sid2, d, 300);

            // сохранили
            sd1.SaveData();

            // загрузили
            SeriesData sd3 = new SeriesData(accDA);
            sd3.LoadData(accID);

            // проверяем
            var series = accDA.GetSeries(accID).ToList();
            Assert.Equal(2, series.Count);
            var s1_ = series.FirstOrDefault(s => s.Key == "key1");
            var s2_ = series.FirstOrDefault(s => s.Key == "key2");
            Assert.Equal(accID, s1_.AccountID);
            Assert.Equal(accID, s2_.AccountID);

            var sv1_ = accDA.GetSeriesValues(s1_.SeriesID).ToList();
            Assert.Equal(2, sv1_.Count);
            var sv2_ = accDA.GetSeriesValues(s2_.SeriesID).ToList();
            Assert.Single(sv2_);
        }

        [Fact]
        public void SubscribeValueRow_fulltest()
        {
            IAccountDA accDA = new AccountDAMock();
            SeriesData sd = new SeriesData(accDA);
            int accID = 10;

            // создали серию и подписали на нее поток
            int sid = sd.OpenSeries("key1");
            BarRow bars = new BarRow(Timeframes.Day, 1);
            sd.SubscribeValueRow(sid, bars.Close, bars.Dates);

            // в поток добавляем 10 значений
            DateTime d = new DateTime(2019, 1, 1, 10, 0, 0);
            for (int i = 0; i < 10; i++)
            {
                bars.AddTick(d, 100, 1);
                d = d.AddDays(1);
            }
            bars.CloseLastBar();

            sd.SetAccount(accID);
            sd.SaveData(); // после сохранения sid уже не актуален, надо использовать series[0].SeriesID

            var series = accDA.GetSeries(accID).ToList();
            Assert.Single(series); // серия одна

            var vals = accDA.GetSeriesValues(series[0].SeriesID).ToList();
            Assert.True(vals.Count == 10); // в серии 10 значений

            // теперь отпишемся от потока
            sd.SubscribeValueRow(series[0].SeriesID, null, null);

            // в поток добавляем еще 10 значений
            for (int i = 0; i < 10; i++)
            {
                bars.AddTick(d, 200, 1);
                d = d.AddDays(1);
            }
            bars.CloseLastBar();

            // снова запишем данные
            sd.SaveData();

            var vals1 = accDA.GetSeriesValues(series[0].SeriesID).ToList();
            Assert.True(vals1.Count == 10); // в серии по прежнему 10 значений
        }
    }
}
