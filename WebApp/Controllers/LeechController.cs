using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulxer;
using Pulxer.Leech;
using System;
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
        private readonly OpenPositions _openPos;

        public LeechController(LeechServerManager lsm, IImportLeech importLeech, IAccountBL accountBL, OpenPositions openPos)
        {
            _lsm = lsm;
            _importLeech = importLeech;
            _accountBL = accountBL;
            _openPos = openPos;
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
            _openPos.RefreshItems();
            ls.DeletePipe(sps.GetPipe()).Wait();

            return Ok();
        }

        [HttpPost("sync")]
        [Authorize]
        public IActionResult Sync()
        {
            var acc = _accountBL.GetRealAccount();
            if (acc == null) return null;

            var ls = _lsm.GetServer();
            if (ls == null) return BadRequest();

            var sps = ls.CreateSyncPipe().Result;
            if (sps == null) return BadRequest();

            _importLeech.SyncAccountDataAsync(sps, acc.AccountID).Wait();
            _openPos.RefreshItems();
            ls.DeletePipe(sps.GetPipe()).Wait();

            return Ok();
        }

        [HttpGet("lastprice/{tickers}")]
        [Authorize]
        public LastPrice[] GetLastPrices(string tickers)
        {
            var ls = _lsm.GetServer();
            if (ls == null) return null;

            var tickerList = Regex.Split(tickers, @"\s*,\s*");
            var tps = ls.CreateTickPipe().Result;
            if (tps == null) return null;

            var prices = tps.GetLastPrices(tickerList).Result;
            ls.DeletePipe(tps.GetPipe()).Wait();

            return prices;
        }

        [HttpGet("lasttickts")]
        [Authorize]
        [Produces("application/json")] // response
        [Consumes("application/json")] // request        
        public string GetLastTickTs()
        {
            var ls = _lsm.GetServer();
            if (ls == null) return null;

            var tps = ls.CreateTickPipe().Result;
            if (tps == null) return null;

            var ts = tps.GetLastTickTs().Result;
            ls.DeletePipe(tps.GetPipe()).Wait();

            if (ts == null) return "";
            
            return ts.Value.ToString("yyyy-MM-ddTHH:mm:ss");
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
