using Platform;
using System.Xml.Linq;
using CommonData = Common.Data;

namespace Common.Interfaces
{
    /// <summary>
    /// Chart da-layer interface
    /// </summary>
    public interface IChartDA
    {
        CommonData.Chart GetChartByID(int chartID);
        CommonData.Chart GetChart(int accountID, int insID, Timeframes tf);
        int CreateChart(int accountID, int insID, Timeframes tf, XDocument xData);
        void UpdateChart(int chartID, XDocument xData);
        void DeleteChartByID(int chartID);
    }
}
