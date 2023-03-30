using Pulxer;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PulxerTest
{
    public class AllTradesEncodingTest
    {
        [Fact]
        public void EncodeDecodeTest_ticks_equalTicks()
        {
            AllTradesEncoding encoding = new AllTradesEncoding(2);

            List<AllTradesTick> ticks = new List<AllTradesTick>();
            DateTime ts = new DateTime(2020, 1, 1);
            ticks.Add(new AllTradesTick() { Ts = ts, Price = 10000, Lots = 10 }); // сразу большая цена
            ticks.Add(new AllTradesTick() { Ts = ts, Price = 100.01m, Lots = 5 });
            ticks.Add(new AllTradesTick() { Ts = ts.AddSeconds(1), Price = 99.01m, Lots = 15 });
            ticks.Add(new AllTradesTick() { Ts = ts.AddSeconds(1), Price = 10099.01m, Lots = 15 }); // большой скачок цены
            ticks.Add(new AllTradesTick() { Ts = ts.AddSeconds(2), Price = 99.01m, Lots = 15 }); // обратный большой скачок цены
            ticks.Add(new AllTradesTick() { Ts = ts.AddSeconds(2), Price = 99.01m, Lots = 150000 }); // большой лот
            ticks.Add(new AllTradesTick() { Ts = ts.AddHours(1), Price = 99.01m, Lots = 10 }); // большой скачок времени
            ticks.Add(new AllTradesTick() { Ts = ts.AddHours(1).AddSeconds(1), Price = 99.01m, Lots = 10 });

            foreach (var tick in ticks) encoding.AddTick(tick.Ts, tick.Price, tick.Lots);

            byte[] encoded = encoding.Encode();

            var ticks1 = encoding.Decode(encoded).ToList();

            Assert.Equal(ticks.Count, ticks1.Count);
            for (int i = 0; i < ticks.Count; i++)
            {
                Assert.Equal(ticks[i].Ts, ticks1[i].Ts);
                Assert.Equal(ticks[i].Price, ticks1[i].Price);
                Assert.Equal(ticks[i].Lots, ticks1[i].Lots);
            }
        }
    }
}
