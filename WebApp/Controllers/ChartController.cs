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

        public ChartController(ChartSystem chartSys)
        {
            _chartSystem = chartSys;
        }

        [HttpGet("{accountID}/{instrumId}/{timeframe}/{from?}/{count?}")]
        [Authorize]
        public TimelineData GetTimelineData(int accountID, int instrumId, int timeframe, int from = 0, int count = 100)
        {
            var cm = _chartSystem.GetChartManager(accountID, instrumId, (Timeframes)timeframe);
            if (cm == null) return null;

            cm.LoadHistoryAsync().Wait();

            var chartData = cm.GetChartData();
            if (chartData == null || chartData.Timeline == null) return null;

            return TimelineData.Generate(chartData.Timeline, from, count);
        }





    }
}
