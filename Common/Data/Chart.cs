using Platform;
using System.Xml.Linq;

namespace Common.Data
{
    /// <summary>
    /// Price chart object
    /// </summary>
    public class Chart
    {
        public Chart()
        {
            ChartID = 0;
            InsID = 0;
            Tf = Timeframes.Min;
            AccountID = 0;
            XmlData = new XDocument();
        }

        public Chart(int accountID, int insID, Timeframes tf, XDocument xd)
        {
            AccountID = accountID; InsID = insID; Tf = tf; XmlData = xd;
        }

        /// <summary>
        /// Primary key
        /// </summary>
        public int ChartID { get; set; }

        /// <summary>
        /// Fin. instrument refer
        /// </summary>
        public int InsID { get; set; }

        /// <summary>
        /// Time frame
        /// </summary>
        public Timeframes Tf { get; set; }

        /// <summary>
        /// Account refer
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Serialized data
        /// </summary>
        public XDocument XmlData { get; set; }
    }
}
