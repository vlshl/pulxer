using Common.Data;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebApp
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstrumController : ControllerBase
    {
        private readonly IInstrumBL _instrumBL = null;
        private readonly ISyncBL _syncBL = null;

        public InstrumController(IInstrumBL instrumBL, ISyncBL syncBL)
        {
            _instrumBL = instrumBL;
            _syncBL = syncBL;
        }

        [HttpGet("{hash?}")]
        [Authorize]
        public string Get(string hash)
        {
            if (hash == null) hash = "";
            return _syncBL.GetInstrums(hash);
        }

        [HttpGet("list")]
        [Authorize]
        public IEnumerable<Instrum> GetList()
        {
            return _instrumBL.GetInstrums();
        }
    }
}
