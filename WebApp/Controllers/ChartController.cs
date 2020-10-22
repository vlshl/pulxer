using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform;
using Pulxer.Drawing;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly ChartSystem _chartSystem;
        private readonly IInstrumBL _instrumBL;

        public ChartController(ChartSystem chartSys, IInstrumBL instrumBL)
        {
            _chartSystem = chartSys;
            _instrumBL = instrumBL;
        }

        [HttpGet("{accountID}/{instrumId}/{timeframe}/timeline/{from?}/{count?}")]
        [Authorize]
        public TimelineData GetTimelineData(int accountID, int instrumId, int timeframe, int from = 0, int? count = null)
        {
            var cm = _chartSystem.GetChartManager(accountID, instrumId, (Timeframes)timeframe);
            if (cm == null) return null;

            cm.LoadHistoryAsync().Wait(); //?????????????????

            var chartData = cm.GetChartData();
            if (chartData == null || chartData.Timeline == null) return null;

            return TimelineData.Generate(chartData.Timeline, from, count);
        }

        [HttpGet("{accountID}/{instrumId}/{timeframe}/visuals")]
        [Authorize]
        public VisualData[] GetVisualData(int accountID, int instrumId, int timeframe)
        {
            var cm = _chartSystem.GetChartManager(accountID, instrumId, (Timeframes)timeframe);
            if (cm == null) return null;

            var chartData = cm.GetChartData();
            if (chartData == null) return null;

            return VisualData.Generate(chartData);
        }

        [HttpGet("{accountID}/{instrumId}/{timeframe}/pricechart/{key}")]
        [Authorize]
        public PriceChartData GetPriceChartData(int accountID, int instrumId, int timeframe, int key)
        {
            var instrum = _instrumBL.GetInstrumByID(instrumId);
            if (instrum == null) return null;

            var cm = _chartSystem.GetChartManager(accountID, instrumId, (Timeframes)timeframe);
            if (cm == null) return null;

            var chartData = cm.GetChartData();
            if (chartData == null) return null;


            return PriceChartData.Generate(key, instrum.Decimals, chartData);
        }
    }
}
