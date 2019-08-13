using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform;
using System.Collections.Generic;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StopOrdersController : ControllerBase
    {
        private IAccountBL _accountBL;

        public StopOrdersController(IAccountBL accountBL)
        {
            _accountBL = accountBL;
        }

        [HttpGet("{ids}")]
        [Authorize]
        public IEnumerable<StopOrder> GetStopOrders(string ids)
        {
            return _accountBL.GetStopOrders(Lib.Str2Ids(ids));
        }
    }
}