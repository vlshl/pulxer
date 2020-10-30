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

        [HttpGet("{accountID}/{instrumId}/{timeframe}/timeline/{from?}")]
        [Authorize]
        public RemoteTimeline GetRemoteTimeline(int accountID, int instrumId, int timeframe, int from = 0)
        {
            var cm = _chartSystem.GetChartManager(accountID, instrumId, (Timeframes)timeframe);
            if (cm == null) return null;

            var chartData = cm.GetChartData();
            if (chartData == null || chartData.Timeline == null) return null;

            return RemoteTimeline.Generate(chartData.Timeline, from);
        }

        [HttpGet("{accountID}/{instrumId}/{timeframe}/chartdata")]
        [Authorize]
        public RemoteChartData GetChartData(int accountID, int instrumId, int timeframe)
        {
            var cm = _chartSystem.GetChartManager(accountID, instrumId, (Timeframes)timeframe);
            if (cm == null) return null;

            var chartData = cm.GetChartData();
            if (chartData == null) return null;

            return RemoteChartData.Generate(chartData);
        }

        [HttpGet("{accountID}/{instrumId}/{timeframe}/pricechart/{key}/{from?}")]
        [Authorize]
        public RemotePriceChart GetRemotePriceChart(int accountID, int instrumId, int timeframe, int key, int from = 0)
        {
            var instrum = _instrumBL.GetInstrumByID(instrumId);
            if (instrum == null) return null;

            var cm = _chartSystem.GetChartManager(accountID, instrumId, (Timeframes)timeframe);
            if (cm == null) return null;

            var chartData = cm.GetChartData();
            if (chartData == null) return null;


            return RemotePriceChart.Generate(key, instrum.Decimals, chartData, from);
        }
    }
}
