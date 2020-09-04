using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        [HttpPost("sync")]
        [Authorize]
        public IActionResult Sync()
        {
            var ls = _lsm.GetServer();
            if (ls == null) return BadRequest();

            var sps = ls.CreateSyncPipe().Result;
            if (sps == null) return BadRequest();

            _importLeech.SyncAccountDataAsync(sps).Wait();
            ls.DeleteSyncPipe().Wait();

            return Ok();
        }

        [HttpGet("lastprice/{tickers}")]
        [Authorize]
        public LastPrice[] GetLastPrices(string tickers)
        {
            var ls = _lsm.GetServer();
            if (ls == null) return null;

            var tickerList = Regex.Split(tickers, @"\s*,\s*");
            var tps = ls.GetTickPipe().Result;
            if (tps == null) return null;

            return tps.GetLastPrices(tickerList).Result;
        }

        [HttpGet("ident")]
        [Authorize]
        public string GetLeechIdentity()
        {
            var ls = _lsm.GetServer();
            if (ls == null) return "";

            var ident = ls.GetRemoteIdentity().Result;
            if (ident == null) return "";

            return ident;
        }
    }
}
