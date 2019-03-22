using System.Xml.Linq;

namespace Common.Interfaces
{
    /// <summary>
    /// Serializable to xml
    /// </summary>
    public interface IXmlSerializable
    {
        XDocument Serialize();
        void Initialize(XDocument xDoc);
    }

    /// <summary>
    /// Serializable to string
    /// </summary>
    public interface IStrSerializable
    {
        string Serialize();
        void Initialize(string str);
    }
}
