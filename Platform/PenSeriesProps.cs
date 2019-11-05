using System;
using System.Xml;

namespace Platform
{
    public class PenSeriesProps : ISeriesProps
    {
        public byte Alpha { get; private set; }
        public byte Red { get; private set; }
        public byte Green { get; private set; }
        public byte Blue { get; private set; }
        public int Thickness { get; private set; }

        public PenSeriesProps()
        {
            Alpha = 255;
            Red = Green = Blue = 0;
            Thickness = 1;
        }

        public PenSeriesProps(int argb, int thickness = 1)
        {
            Blue = (byte)(argb & 0xff);
            Green = (byte)((argb >> 8) & 0xff);
            Red = (byte)((argb >> 16) & 0xff);
            Alpha = (byte)((argb >> 24) & 0xff);
            Thickness = thickness;
        }

        public PenSeriesProps(byte red, byte green, byte blue, byte alpha = 255, int thickness = 1)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
            Thickness = thickness;
        }

        public void Initialize(XmlDocument xd)
        {
            try
            {
                int argb;
                if (int.TryParse(xd.DocumentElement.Attributes["Color"].Value, out argb))
                {
                    Blue = (byte)(argb & 0xff);
                    Green = (byte)((argb >> 8) & 0xff);
                    Red = (byte)((argb >> 16) & 0xff);
                    Alpha = (byte)((argb >> 24) & 0xff);
                }

                int thickness;
                if (int.TryParse(xd.DocumentElement.Attributes["Thickness"].Value, out thickness))
                {
                    Thickness = thickness;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public XmlDocument Serialize()
        {
            var xd = new XmlDocument();
            int color = (Alpha << 24) + (Red << 16) + (Green << 8) + Blue;
            string xml = string.Format("<PenSeriesProps Color=\"{0}\" Thickness=\"{1}\" />", color, Thickness);
            xd.LoadXml(xml);

            return xd;
        }
    }
}
