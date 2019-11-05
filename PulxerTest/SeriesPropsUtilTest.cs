using Xunit;

namespace Pulxer.Test
{
    public class SeriesPropsUtilTest
    {
        [Fact]
        public void CreateSerialize()
        {
            string xml = "<PenSeriesProps Color=\"123456\" Thickness=\"55\" />";
            var sp = SeriesPropsUtil.Create(xml);
            string xml1 = SeriesPropsUtil.Serialize(sp);

            Assert.Equal(xml, xml1);
        }
    }
}
