using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform;
using Pulxer;
using System.Collections.Generic;
using System.Linq;

namespace WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : ControllerBase
    {
        private IPositionBL _positionBL;

        public PositionsController(IPositionBL positionBL)
        {
            _positionBL = positionBL;
        }

        [HttpGet("{ids}")]
        [Authorize]
        public IEnumerable<RemotePosition> GetPositions(string ids)
        {
            var posList = _positionBL.GetPositions(Lib.Str2Ids(ids));
            var posTrades = _positionBL.GetPosTrades(posList.Select(r => r.PosID).ToList());

            var list = posList.Select(r => new RemotePosition()
            {
                PosID = r.PosID,
                InsID = r.InsID,
                Count = r.Count,
                OpenTime = r.OpenTime,
                OpenPrice = r.OpenPrice,
                CloseTime = r.CloseTime,
                ClosePrice = r.ClosePrice,
                PosType = (byte)r.PosType,
                AccountID = r.AccountID,
                TradeIDs = posTrades.Where(p => p.PosID == r.PosID).Select(p => p.TradeID).ToList()
            }).ToList();

            return list;
        }
    }
}