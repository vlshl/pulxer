using Common.Data;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WebApp.Controllers;

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

        [HttpGet("favs")]
        [Authorize]
        public int[] GetFavInstrumIds()
        {
            return _instrumBL.GetFavInstrumIds(this.GetUserId());
        }

        [HttpGet("actives")]
        [Authorize]
        public int[] GetActiveInstrumIds()
        {
            return _instrumBL.GetActiveInstrumIds();
        }

        [HttpPost("addfav/{instrumId}")]
        [Authorize]
        public int[] AddFavInstrum(int instrumId)
        {
            return _instrumBL.AddFavorite(this.GetUserId(), instrumId);
        }

        [HttpPost("remfav/{instrumId}")]
        [Authorize]
        public int[] RemoveFavInstrum(int instrumId)
        {
            return _instrumBL.RemoveFavorite(this.GetUserId(), instrumId);
        }
    }
}
