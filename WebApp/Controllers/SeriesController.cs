using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform;
using System.Collections.Generic;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeriesController : ControllerBase
    {
        private IAccountBL _accountBL;

        public SeriesController(IAccountBL accountBL)
        {
            _accountBL = accountBL;
        }

        [HttpGet("{seriesID}/values/{skipCount?}/{takeCount?}")]
        [Authorize]
        public IEnumerable<SeriesValue> GetValues(int seriesID, int? skipCount, int? takeCount)
        {
            if (skipCount == null || skipCount < 0) skipCount = 0;
            if (takeCount != null && takeCount.Value < 0) takeCount = 0;
            return _accountBL.GetValues(seriesID, skipCount.Value, takeCount);
        }

        [HttpGet("{seriesID}/values/count")]
        [Authorize]
        public int GetValuesCount(int seriesID)
        {
            return _accountBL.GetValuesCount(seriesID);
        }
    }
}