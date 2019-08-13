using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform;
using System.Collections.Generic;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private IAccountBL _accountBL;

        public OrdersController(IAccountBL accountBL)
        {
            _accountBL = accountBL;
        }

        [HttpGet("{ids}")]
        [Authorize]
        public IEnumerable<Order> GetOrders(string ids)
        {
            return _accountBL.GetOrders(Lib.Str2Ids(ids));
        }
    }
}