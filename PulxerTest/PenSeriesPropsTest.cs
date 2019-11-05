using Platform;
using System.Xml;
using Xunit;

namespace Pulxer.Test
{
    public class PenSeriesPropsTest
    {
        [Fact]
        public void SerializeInitialize()
        {
            PenSeriesProps sp = new PenSeriesProps();
            int color = 10 + 20 * 256 + 30 * 256 * 256 + 40 * 256 * 256 * 256;
            int thickness = 5;
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(string.Format("<PenSeriesProps Color = \"{0}\" Thickness=\"{1}\" />", color, thickness));
            sp.Initialize(xd);

            Assert.Equal(10, sp.Blue);
            Assert.Equal(20, sp.Green);
            Assert.Equal(30, sp.Red);
            Assert.Equal(40, sp.Alpha);
            Assert.Equal(thickness, sp.Thickness);

            var xd1 = sp.Serialize();
            Assert.Equal("PenSeriesProps", xd1.DocumentElement.Name);
            Assert.Equal(color.ToString(), xd1.DocumentElement.Attributes["Color"].Value);
            Assert.Equal(thickness.ToString(), xd1.DocumentElement.Attributes["Thickness"].Value);
        }
    }
}
