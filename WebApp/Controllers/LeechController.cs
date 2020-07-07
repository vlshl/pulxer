using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            var ls = _lsm.GetLeechServer(account);
            if (ls == null) return NotFound();

            var sps = ls.CreateSyncPipe().Result;
            if (sps == null) return BadRequest();

            _importLeech.SyncAccountDataAsync(sps).Wait();

            // ls.DeleteSyncPipe(); // еще не реализовано

            return Ok();
        }
    }
}
