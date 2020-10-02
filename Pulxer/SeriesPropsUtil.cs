using Platform;
using System.IO;
using System.Xml;
using Pulxer.Drawing;

namespace Pulxer
{
    public class SeriesPropsUtil
    {
        public static ISeriesProps Create(string xml)
        {
            if (string.IsNullOrEmpty(xml)) return null;

            XmlDocument xDoc = new XmlDocument();
            xDoc.LoadXml(xml);
            if (xDoc.DocumentElement.Name == "PenSeriesProps")
            {
                var sp = new PenSeriesProps();
                sp.Initialize(xDoc);

                return sp;
            }

            return null;
        }

        public static string Serialize(ISeriesProps sp)
        {
            if (sp == null) return "";

            XmlDocument xd = sp.Serialize();
            StringWriter sw = new StringWriter();
            XmlTextWriter xw = new XmlTextWriter(sw);
            xd.WriteTo(xw);

            return sw.ToString();
        }
    }
}
