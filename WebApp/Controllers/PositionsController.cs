using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Platform;
using Pulxer;
using System;
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
        private readonly OpenPositions _openPos;

        public PositionsController(IPositionBL positionBL, IAccountBL accountBL, OpenPositions openPos)
        {
            _positionBL = positionBL;
            _accountBL = accountBL;
            _openPos = openPos;
        }

        [HttpGet("{ids}")]
        [Authorize]
        public IEnumerable<TradePosition> GetPositions(string ids)
        {
            var posList = _positionBL.GetPositions(Lib.Str2Ids(ids));
            var posTrades = _positionBL.GetPosTrades(posList.Select(r => r.PosID).ToList());

            var list = posList.Select(r => new TradePosition()
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
        public IEnumerable<OpenPosItem> GetOpenPositions()
        {
            return _openPos.GetPositions();
        }

        [HttpGet("close")]
        [Authorize]
        public IEnumerable<TradePosition> GetClosePositions()
        {
            var acc = _accountBL.GetRealAccount();
            if (acc == null) return null;

            var posList = _positionBL.GetClosedPositions(acc.AccountID);
            var posTrades = _positionBL.GetPosTrades(posList.Select(r => r.PosID).ToList());

            var list = posList.Select(r => new TradePosition()
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
            _openPos.RefreshItems();

            return Ok();
        }

        [HttpPost("refresh")]
        [Authorize]
        public IActionResult RefreshPos()
        {
            var acc = _accountBL.GetRealAccount();
            if (acc == null) return BadRequest();

            _positionBL.RefreshPositions(acc.AccountID);
            _openPos.RefreshItems();

            return Ok();
        }
    }
}