using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulxer.Leech;
using System.Text.RegularExpressions;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LeechController : ControllerBase
    {
        private readonly LeechServerManager _lsm;
        private readonly IImportLeech _importLeech;
        private readonly IAccountBL _accountBL;

        public LeechController(LeechServerManager lsm, IImportLeech importLeech, IAccountBL accountBL)
        {
            _lsm = lsm;
            _importLeech = importLeech;
            _accountBL = accountBL;
        }

        [HttpPost("fullsync")]
        [Authorize]
        public IActionResult FullSync()
        {
            var ls = _lsm.GetServer();
            if (ls == null) return BadRequest();

            var sps = ls.CreateSyncPipe().Result;
            if (sps == null) return BadRequest();

            _importLeech.FullSyncAccountDataAsync(sps).Wait();
            ls.DeleteSyncPipe().Wait();

            return Ok();
        }

        [HttpPost("sync")]
        [Authorize]
        public IActionResult Sync()
        {
            var ls = _lsm.GetServer();
            if (ls == null) return BadRequest();

            var sps = ls.CreateSyncPipe().Result;
            if (sps == null) return BadRequest();

            var acc = _accountBL.GetRealAccount();
            if (acc == null) return null;

            _importLeech.SyncAccountDataAsync(sps, acc.AccountID).Wait();
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

            var prices = tps.GetLastPrices(tickerList).Result;
            ls.DeleteTickPipe().Wait();

            return prices;
        }

        [HttpGet("ident")]
        [Authorize]
        [Produces("application/json")] // response
        [Consumes("application/json")] // request        
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
