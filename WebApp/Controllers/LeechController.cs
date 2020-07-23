using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pulxer.Leech;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeechController : ControllerBase
    {
        private LeechServerManager _lsm;
        private IImportLeech _importLeech;

        public LeechController(LeechServerManager lsm, IImportLeech importLeech)
        {
            _lsm = lsm;
            _importLeech = importLeech;
        }

        [HttpPost("sync/{account}")]
        [Authorize]
        public IActionResult Sync(string account)
        {
            var ls = _lsm.GetServer(account);
            if (ls == null) return NotFound();

            var sps = ls.CreateSyncPipe().Result;
            if (sps == null) return BadRequest();

            _importLeech.SyncAccountDataAsync(sps).Wait();
            ls.DeleteSyncPipe().Wait();

            return Ok();
        }

        [HttpGet("tick/{account}/{insIds}")]
        [Authorize]
        public PriceData[] GetLastTicks(string account, string insIds)
        {
            var ls = _lsm.GetServer(account);
            if (ls == null) return null;

            var tps = ls.GetTickPipe().Result;
            if (tps == null) return null;

            var ticks = tps.GetLastTicks(insIds).Result;

            return ticks.Select(t => new PriceData(t)).ToArray();
        }
    }

    public struct PriceData
    {
        public int InsId { get; set; }
        public int Date { get; set; }
        public int Time { get; set; }
        public decimal Price { get; set; }
        public int Lots { get; set; }

        public PriceData(Tick tick)
        {
            this.InsId = tick.InsID;
            this.Date = tick.Time.Year * 10000 + tick.Time.Month * 100 + tick.Time.Day;
            this.Time = tick.Time.Hour * 10000 + tick.Time.Minute * 100 + tick.Time.Second;
            this.Price = tick.Price;
            this.Lots = tick.Lots;
        }
    }
}
