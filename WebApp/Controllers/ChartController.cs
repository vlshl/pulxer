using Common.Data;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform;
using Pulxer.Drawing;
using Pulxer.Leech;
using System.Linq;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private readonly ChartSystem _chartSystem;
        private readonly IInstrumBL _instrumBL;
        private readonly LeechServerManager _lsm;
        private readonly ITickDispatcher _tickDisp;

        public ChartController(ITickDispatcher tickDisp, LeechServerManager lsm, ChartSystem chartSys, IInstrumBL instrumBL)
        {
            _tickDisp = tickDisp;
            _lsm = lsm;
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

            if (chartData.IsDynamic)
            {
                LoadLastTicks(instrum);
            }

            return RemotePriceChart.Generate(key, instrum.Decimals, chartData, from);
        }

        private void LoadLastTicks(Instrum instrum)
        {
            var ls = _lsm.GetServer();
            if (ls == null) return;

            var tps = ls.GetTickPipe().Result;
            if (tps == null) return;

            int count = _tickDisp.GetTicksCount(instrum.InsID);
            var ticks = tps.GetLastTicks(_tickDisp.CurrentDate, instrum, count).Result;
            if (ticks != null && ticks.Any()) _tickDisp.AddTicks(ticks);
            ls.DeleteTickPipe().Wait();
        }
    }
}
