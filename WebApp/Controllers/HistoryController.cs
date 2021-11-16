using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Platform;
using Pulxer;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly IInstrumBL _instrumBL = null;
        private readonly IInsStoreBL _insStoreBL = null;

        public HistoryController(IInstrumBL instrumBL, IInsStoreBL insStoreBL)
        {
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
        }

        [HttpGet("{ticker}/{tf}/{y4md1}/{y4md2?}")]
        [Authorize]
        public ActionResult<HistData> GetHistData(string ticker, string tf, int y4md1, int? y4md2)
        {
            DateTime? d1 = Lib.IntToDateTime(y4md1, false); // округляем в начало
            DateTime? d2 = y4md2 != null ? Lib.IntToDateTime(y4md2.Value, true) : Lib.IntToDateTime(y4md1, true); // округляем в конец
            if (d1 == null || d2 == null) return BadRequest("Dates format error.");

            var instrum = _instrumBL.GetInstrum(ticker);
            if (instrum == null) return BadRequest("Ticker not found.");

            var timeframe = TfHelper.Parse(tf);
            if (timeframe == null) return BadRequest("Invalid timeframe.");

            BarRow bars = new BarRow(timeframe.Value, instrum.InsID);
            _insStoreBL.LoadHistoryAsync(bars, instrum.InsID, d1.Value, d2.Value).Wait();

            var times = (from b in bars.Bars select b.Time.ToString("yyyy-MM-ddTHH:mm:ss")).ToArray();
            var opens = (from b in bars.Bars select b.Open).ToArray();
            var highs = (from b in bars.Bars select b.High).ToArray();
            var lows = (from b in bars.Bars select b.Low).ToArray();
            var closes = (from b in bars.Bars select b.Close).ToArray();
            var volumes = (from b in bars.Bars select b.Volume).ToArray();

            return new HistData() { DateTime = times, Open = opens, High = highs, Close = closes, Low = lows, Volume = volumes };
        }
    }

    public class HistData
    {
        public string[] DateTime { get; set; }
        public decimal[] Open { get; set; }
        public decimal[] High { get; set; }
        public decimal[] Low { get; set; }
        public decimal[] Close { get; set; }
        public long[] Volume { get; set; }
    }
}
