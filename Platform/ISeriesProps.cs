using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Platform
{
    public interface ISeriesProps
    {
        XmlDocument Serialize();
        void Initialize(XmlDocument xd);
    }
}
