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
        private IAccountBL _accountBL;

        public PositionsController(IPositionBL positionBL, IAccountBL accountBL)
        {
            _positionBL = positionBL;
            _accountBL = accountBL;
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

        [HttpGet("open")]
        [Authorize]
        public IEnumerable<RemotePosition> GetOpenPositions()
        {
            return GetPositions(true);
        }

        [HttpGet("close")]
        [Authorize]
        public IEnumerable<RemotePosition> GetClosPositions()
        {
            return GetPositions(false);
        }

        private IEnumerable<RemotePosition> GetPositions(bool isOpened)
        {
            var acc = _accountBL.GetRealAccount();
            if (acc == null) return null;

            var posList = isOpened ? _positionBL.GetOpenedPositions(acc.AccountID) : _positionBL.GetClosedPositions(acc.AccountID);
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

        [HttpPost("clear")]
        [Authorize]
        public IActionResult ClearPos()
        {
            var acc = _accountBL.GetRealAccount();
            if (acc == null) return BadRequest();
            
            _positionBL.ClearPositions(acc.AccountID);
            return Ok();
        }

        [HttpPost("refresh")]
        [Authorize]
        public IActionResult RefreshPos()
        {
            var acc = _accountBL.GetRealAccount();
            if (acc == null) return BadRequest();

            _positionBL.RefreshPositions(acc.AccountID);
            return Ok();
        }
    }
}