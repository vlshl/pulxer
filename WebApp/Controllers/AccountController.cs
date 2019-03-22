using Common.Data;
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
    public class AccountController : ControllerBase
    {
        private IAccountBL _accountBL;
        private IPositionBL _positionBL;
        private IRepositoryBL _reposBL;

        public AccountController(IAccountBL accountBL, IPositionBL positionBL, IRepositoryBL reposBL)
        {
            _accountBL = accountBL;
            _positionBL = positionBL;
            _reposBL = reposBL;
        }

        #region Account
        [HttpGet]
        [Authorize]
        public IEnumerable<AccountListItem> GetList()
        {
            return _accountBL.GetAccountList();
        }

        [HttpGet("{id}")]
        [Authorize]
        public Account GetByID(int id)
        {
            return _accountBL.GetAccountByID(id);
        }
        #endregion

        #region Cash
        [HttpGet("{accountID}/cash")]
        [Authorize]
        public Cash GetCash(int accountID)
        {
            return _accountBL.GetCash(accountID);
        }
        #endregion

        #region Order
        [HttpGet("{accountID}/orders/{fromID?}")]
        [Authorize]
        public IEnumerable<Order> GetOrders(int accountID, int? fromID)
        {
            return _accountBL.GetOrders(accountID, fromID);
        }

        [HttpGet("orders/{ids}")]
        [Authorize]
        public IEnumerable<Order> GetOrders(string ids)
        {
            return _accountBL.GetOrders(Lib.Str2Ids(ids));
        }
        #endregion

        #region StopOrder
        [HttpGet("{accountID}/stoporders/{fromID?}")]
        [Authorize]
        public IEnumerable<StopOrder> GetStopOrders(int accountID, int? fromID)
        {
            return _accountBL.GetStopOrders(accountID, fromID);
        }

        [HttpGet("stoporders/{ids}")]
        [Authorize]
        public IEnumerable<StopOrder> GetStopOrders(string ids)
        {
            return _accountBL.GetStopOrders(Lib.Str2Ids(ids));
        }
        #endregion

        #region Trade
        [HttpGet("{accountID}/trades/{fromID?}")]
        [Authorize]
        public IEnumerable<Trade> GetTrades(int accountID, int? fromID)
        {
            return _accountBL.GetTrades(accountID, fromID);
        }
        #endregion

        #region Holdings
        [HttpGet("{accountID}/holdings")]
        [Authorize]
        public IEnumerable<Holding> GetHoldings(int accountID)
        {
            return _accountBL.GetHoldings(accountID);
        }
        #endregion

        #region Position
        [HttpGet("{accountID}/positions/{fromID?}")]
        [Authorize]
        public IEnumerable<RemotePosition> GetPositions(int accountID, int? fromID)
        {
            var posList = _positionBL.GetAllPositions(accountID);
            if (fromID != null) posList = posList.Where(r => r.PosID >= fromID);
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

        [HttpGet("positions/{ids}")]
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
        #endregion

        #region Account metadata
        [HttpGet("{accountID}/meta")]
        [Authorize]
        public string GetAccountMeta(int accountID)
        {
            var json = _reposBL.GetStringParam(TestRun.ACCOUNT_META + accountID.ToString());
            if (json == null) json = "";
            return json;
        }
        #endregion
    }
}