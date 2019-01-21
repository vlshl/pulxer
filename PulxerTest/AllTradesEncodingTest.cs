using Pulxer;
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
            ticks.Add(new AllTradesTick() { Second = 1000, Price = 10000, Lots = 10 }); // сразу большая цена
            ticks.Add(new AllTradesTick() { Second = 1000, Price = 100.01m, Lots = 5 });
            ticks.Add(new AllTradesTick() { Second = 1001, Price = 99.01m, Lots = 15 });
            ticks.Add(new AllTradesTick() { Second = 1001, Price = 10099.01m, Lots = 15 }); // большой скачок цены
            ticks.Add(new AllTradesTick() { Second = 1002, Price = 99.01m, Lots = 15 }); // обратный большой скачок цены
            ticks.Add(new AllTradesTick() { Second = 1002, Price = 99.01m, Lots = 150000 }); // большой лот
            ticks.Add(new AllTradesTick() { Second = 10000, Price = 99.01m, Lots = 10 }); // большой скачок времени
            ticks.Add(new AllTradesTick() { Second = 10001, Price = 99.01m, Lots = 10 });

            foreach (var tick in ticks) encoding.AddTick(tick.Second, tick.Price, tick.Lots);

            byte[] encoded = encoding.Encode();

            var ticks1 = encoding.Decode(encoded).ToList();

            Assert.Equal(ticks.Count, ticks1.Count);
            for (int i = 0; i < ticks.Count; i++)
            {
                Assert.Equal(ticks[i].Second, ticks1[i].Second);
                Assert.Equal(ticks[i].Price, ticks1[i].Price);
                Assert.Equal(ticks[i].Lots, ticks1[i].Lots);
            }
        }
    }
}
