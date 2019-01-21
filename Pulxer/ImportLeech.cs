using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer
{
    public class ImportLeech : IImportLeech
    {
        private readonly IInstrumDA _instrumDA;
        private readonly IAccountDA _accountDA;
        private readonly ILeechDA _leechDA;
        private readonly IConfig _config = null;
        private readonly ITickHistoryDA _tickHistoryDA = null;
        private Dictionary<int, int> _instrum_rid_lid;
        private Dictionary<int, int> _account_rid_lid;
        private Dictionary<int, int> _stoporder_rid_lid;
        private Dictionary<int, int> _order_rid_lid;
        private Dictionary<int, int> _trade_rid_lid;
        private readonly IReplicationBL _replBL = null;

        public ImportLeech(IInstrumDA instrumDA, IAccountDA accountDA, ILeechDA leechDA, IConfig config, ITickHistoryDA tickHistoryDA,
            IReplicationBL replBL)
        {
            _instrumDA = instrumDA;
            _accountDA = accountDA;
            _leechDA = leechDA;
            _config = config;
            _tickHistoryDA = tickHistoryDA;
            _replBL = replBL;

            _instrum_rid_lid = new Dictionary<int, int>();
            _account_rid_lid = new Dictionary<int, int>();
            _stoporder_rid_lid = new Dictionary<int, int>();
            _order_rid_lid = new Dictionary<int, int>();
            _trade_rid_lid = new Dictionary<int, int>();
        }

        public async Task SyncAccountDataAsync()
        {
            await Task.Run(() =>
            {
                _instrum_rid_lid = _replBL.GetReplications(ReplObjects.Instrum);
                _account_rid_lid = _replBL.GetReplications(ReplObjects.Account);
                _stoporder_rid_lid = _replBL.GetReplications(ReplObjects.StopOrder);
                _order_rid_lid = _replBL.GetReplications(ReplObjects.Order);
                _trade_rid_lid = _replBL.GetReplications(ReplObjects.Trade);

                SyncInstrums();
                SyncAccounts();

                foreach (var rAccID in _account_rid_lid.Keys)
                {
                    int lAccID = _account_rid_lid[rAccID];
                    SyncStopOrders(lAccID, rAccID);
                    SyncOrders(lAccID, rAccID);
                    SyncTrades(lAccID, rAccID);
                    SyncHoldings(lAccID, rAccID);
                    SyncCash(lAccID, rAccID);
                }

                _replBL.UpdateReplications(ReplObjects.Instrum, _instrum_rid_lid);
                _replBL.UpdateReplications(ReplObjects.Account, _account_rid_lid);
                _replBL.UpdateReplications(ReplObjects.StopOrder, _stoporder_rid_lid);
                _replBL.UpdateReplications(ReplObjects.Order, _order_rid_lid);
                _replBL.UpdateReplications(ReplObjects.Trade, _trade_rid_lid);
            });
        }

        /// <summary>
        /// Синхронизация фин инструментов
        /// </summary>
        private void SyncInstrums()
        {
            var remInstrums = _leechDA.GetInstrumList();
            var instrums = _instrumDA.GetInstrums();

            foreach (var rIns in remInstrums)
            {
                if (_instrum_rid_lid.ContainsKey(rIns.InsID))
                {
                    int lid = _instrum_rid_lid[rIns.InsID];
                    var ins = instrums.FirstOrDefault(r => r.InsID == lid);
                    if (ins != null) // локальный найден
                    {
                        if (ins.ShortName != rIns.ShortName ||
                            ins.Name != rIns.Name ||
                            ins.LotSize != rIns.LotSize ||
                            ins.Decimals != rIns.Decimals ||
                            ins.PriceStep != rIns.PriceStep)
                        {
                            _instrumDA.UpdateInstrum(ins.InsID, rIns.ShortName, rIns.Name,
                                rIns.LotSize, rIns.Decimals, rIns.PriceStep);
                        }
                    }
                    else // соответствие есть, но локальный не найден, значит соответствие уже не действительно
                    {
                        int insID = _instrumDA.InsertInstrum(
                            rIns.Ticker,
                            rIns.ShortName,
                            rIns.Name,
                            rIns.LotSize,
                            rIns.Decimals,
                            rIns.PriceStep);
                        _instrum_rid_lid[rIns.InsID] = insID; // прописываем новый ключ
                    }
                }
                else // соответствие не найдено
                {
                    int insID = _instrumDA.InsertInstrum(
                        rIns.Ticker,
                        rIns.ShortName,
                        rIns.Name,
                        rIns.LotSize,
                        rIns.Decimals,
                        rIns.PriceStep);
                    _instrum_rid_lid.Add(rIns.InsID, insID);
                }
            }

            // отследим ситуацию, когда удаленный объект удален, а локальный остался
            // если локальный объект есть, а репликации для него нет, то значит этот объект не реплицировался и мы его не трогаем
            // если локальный объект есть и репликация для него есть, то мы смотрим remote-объект
            // если remote-объект есть, то здесь ничего не делаем (эту ситуацию мы обработали выше)
            // если remote-объекта нет, то локальный объект НЕ удаляем и не меняем, а просто удаляем для него репликацию
            // удалить локальный объект нельзя, т.к. на него могут быть ссылки, т.е. он остается, но больше не будет реплицироваться
            // если в удаленной базе объект снова возникнет под прежним id (что вряд-ли возможно), 
            // то локально будет создан новый объект, а старый также останется
            foreach (var ins in instrums)
            {
                if (!_instrum_rid_lid.ContainsValue(ins.InsID)) continue; // запись не реплицировалась и мы ее пропускаем

                var repl = _instrum_rid_lid.FirstOrDefault(r => r.Value == ins.InsID); // обязательно будет не null
                var found = remInstrums.FirstOrDefault(r => r.InsID == repl.Key);
                if (found == null) // соответствующей remote-записи нет, значит локальную запись удаляем и убираем для нее репликацию
                {
                    _instrum_rid_lid.Remove(repl.Key);
                }
            }
        }

        /// <summary>
        /// Синхронизация счетов
        /// </summary>
        private void SyncAccounts()
        {
            var remAccounts = _leechDA.GetAccountList();
            var accounts = _accountDA.GetAccounts();

            foreach (var racc in remAccounts)
            {
                if (_account_rid_lid.ContainsKey(racc.AccountID))
                {
                    int lid = _account_rid_lid[racc.AccountID];
                    var account = accounts.FirstOrDefault(r => r.AccountID == lid);
                    if (account != null) // локальный найден
                    {
                        bool isUpdate = false;
                        if (account.AccountType != racc.AccountType) { account.AccountType = racc.AccountType; isUpdate = true; }
                        if (account.Code != racc.Code) { account.Code = racc.Code; isUpdate = true; }
                        if (account.Name != racc.Name) { account.Name = racc.Name; isUpdate = true; }
                        if (account.CommPerc != racc.CommPerc) { account.CommPerc = racc.CommPerc; isUpdate = true; }
                        if (account.IsShortEnable != racc.IsShortEnable) { account.IsShortEnable = racc.IsShortEnable; isUpdate = true; }

                        if (isUpdate)
                        {
                            _accountDA.UpdateAccount(account);
                        }
                    }
                    else // соответствие есть, но локальный не найден, значит соответствие уже не действительно
                    {
                        var acc = _accountDA.CreateAccount(racc.Code, racc.Name, racc.CommPerc, racc.IsShortEnable, racc.AccountType);
                        _account_rid_lid[racc.AccountID] = acc.AccountID; // прописываем новый ключ
                    }
                }
                else // соответствие не найдено
                {
                    var acc = _accountDA.CreateAccount(racc.Code, racc.Name, racc.CommPerc, racc.IsShortEnable, racc.AccountType);
                    _account_rid_lid.Add(racc.AccountID, acc.AccountID);
                }
            }
        }

        /// <summary>
        /// Синхронизация стоп-заявок
        /// </summary>
        private void SyncStopOrders(int localAccountID, int remoteAccountID)
        {
            var remStopOrders = _leechDA.GetStopOrderList(remoteAccountID);
            var stopOrders = _accountDA.GetStopOrders(localAccountID);

            foreach (var rso in remStopOrders)
            {
                int insID = 0; // пытаемся сопоставить инструмент
                if (_instrum_rid_lid.ContainsKey(rso.InsID)) insID = _instrum_rid_lid[rso.InsID];
                if (insID == 0) continue; // если не смогли сопоставить инструмент, то ничего больше сделать не можем

                if (_stoporder_rid_lid.ContainsKey(rso.StopOrderID))
                {
                    int lid = _stoporder_rid_lid[rso.StopOrderID];
                    var stopOrder = stopOrders.FirstOrDefault(r => r.StopOrderID == lid);
                    if (stopOrder != null) // локальный найден
                    {
                        // сравниваем все по полной, хотя в реальности объект не может так сильно меняться
                        bool isUpdate = false;
                        if (stopOrder.InsID != insID) { stopOrder.InsID = insID; isUpdate = true; }
                        if (stopOrder.AlertPrice != rso.AlertPrice) { stopOrder.AlertPrice = rso.AlertPrice; isUpdate = true; }
                        if (stopOrder.BuySell != rso.BuySell) { stopOrder.BuySell = rso.BuySell; isUpdate = true; }
                        if (stopOrder.LotCount != rso.LotCount) { stopOrder.LotCount = rso.LotCount; isUpdate = true; }
                        if (stopOrder.Status != rso.Status) { stopOrder.Status = rso.Status; isUpdate = true; }
                        if (stopOrder.StopOrderNo != rso.StopOrderNo) { stopOrder.StopOrderNo = rso.StopOrderNo; isUpdate = true; }
                        if (stopOrder.StopType != rso.StopType) { stopOrder.StopType = rso.StopType; isUpdate = true; }
                        if (stopOrder.Time != rso.Time) { stopOrder.Time = rso.Time; isUpdate = true; }
                        if (stopOrder.Price != rso.Price) { stopOrder.Price = rso.Price; isUpdate = true; }
                        if (stopOrder.EndTime != rso.EndTime) { stopOrder.EndTime = rso.EndTime; isUpdate = true; }
                        if (stopOrder.CompleteTime != rso.CompleteTime) { stopOrder.CompleteTime = rso.CompleteTime; isUpdate = true; }

                        if (isUpdate)
                        {
                            _accountDA.UpdateStopOrder(stopOrder);
                        }
                    }
                    else // соответствие есть, но локальный не найден, значит соответствие уже не действительно
                    {
                        _stoporder_rid_lid.Remove(rso.StopOrderID); // удаляем старое соответствие
                        var so = _accountDA.CreateStopOrder(localAccountID, rso.Time, insID, rso.BuySell, rso.StopType, rso.EndTime, rso.AlertPrice, rso.Price,
                            rso.LotCount, rso.Status, rso.CompleteTime, rso.StopOrderNo);
                        _stoporder_rid_lid.Add(rso.StopOrderID, so.StopOrderID);
                    }
                }
                else // соответствие не найдено
                {
                    var so = _accountDA.CreateStopOrder(localAccountID, rso.Time, insID, rso.BuySell, rso.StopType, rso.EndTime, rso.AlertPrice, rso.Price,
                        rso.LotCount, rso.Status, rso.CompleteTime, rso.StopOrderNo);
                    _stoporder_rid_lid.Add(rso.StopOrderID, so.StopOrderID);
                }
            }
        }

        /// <summary>
        /// Синхронизация заявок
        /// </summary>
        private void SyncOrders(int localAccountID, int remoteAccountID)
        {
            var remOrders = _leechDA.GetOrderList(remoteAccountID);
            var orders = _accountDA.GetOrders(localAccountID);

            foreach (var rOrd in remOrders)
            {
                int insID = 0; // пытаемся сопоставить инструмент
                if (_instrum_rid_lid.ContainsKey(rOrd.InsID)) insID = _instrum_rid_lid[rOrd.InsID];
                if (insID == 0) continue; // если не смогли сопоставить инструмент, то ничего больше сделать не можем

                // ссылка на стоп-заявку
                int? soID = null;
                if (rOrd.StopOrderID != null && _stoporder_rid_lid.ContainsKey(rOrd.StopOrderID.Value))
                {
                    soID = _stoporder_rid_lid[rOrd.StopOrderID.Value];
                }

                if (_order_rid_lid.ContainsKey(rOrd.OrderID))
                {
                    int lid = _order_rid_lid[rOrd.OrderID];
                    var order = orders.FirstOrDefault(r => r.OrderID == lid);
                    if (order != null) // локальный найден
                    {
                        // сравниваем все по полной, хотя в реальности объект не может так сильно меняться
                        bool isUpdate = false;
                        if (order.InsID != insID) { order.InsID = insID; isUpdate = true; }
                        if (order.BuySell != rOrd.BuySell) { order.BuySell = rOrd.BuySell; isUpdate = true; }
                        if (order.LotCount != rOrd.LotCount) { order.LotCount = rOrd.LotCount; isUpdate = true; }
                        if (order.Status != rOrd.Status) { order.Status = rOrd.Status; isUpdate = true; }
                        if (order.OrderNo != rOrd.OrderNo) { order.OrderNo = rOrd.OrderNo; isUpdate = true; }
                        if (order.Time != rOrd.Time) { order.Time = rOrd.Time; isUpdate = true; }
                        if (order.Price != rOrd.Price) { order.Price = rOrd.Price; isUpdate = true; }
                        if (order.StopOrderID != soID) { order.StopOrderID = soID; isUpdate = true; }

                        if (isUpdate)
                        {
                            _accountDA.UpdateOrder(order);
                        }
                    }
                    else // соответствие есть, но локальный не найден, значит соответствие уже не действительно
                    {
                        _order_rid_lid.Remove(rOrd.OrderID); // удаляем старое соответствие
                        var ord = _accountDA.CreateOrder(localAccountID, rOrd.Time, insID, rOrd.BuySell, rOrd.LotCount, rOrd.Price, rOrd.Status, soID, rOrd.OrderNo);
                        _order_rid_lid.Add(rOrd.OrderID, ord.OrderID);
                    }
                }
                else // соответствие не найдено
                {
                    var ord = _accountDA.CreateOrder(localAccountID, rOrd.Time, insID, rOrd.BuySell, rOrd.LotCount, rOrd.Price, rOrd.Status, soID, rOrd.OrderNo);
                    _order_rid_lid.Add(rOrd.OrderID, ord.OrderID);
                }
            }
        }

        /// <summary>
        /// Синхронизация сделок
        /// </summary>
        private void SyncTrades(int localAccountID, int remoteAccountID)
        {
            var remTrades = _leechDA.GetTradeList(remoteAccountID);
            var trades = _accountDA.GetTrades(localAccountID);

            foreach (var rtrd in remTrades)
            {
                int insID = 0; // пытаемся сопоставить инструмент
                if (_instrum_rid_lid.ContainsKey(rtrd.InsID)) insID = _instrum_rid_lid[rtrd.InsID];
                if (insID == 0) continue; // если не смогли сопоставить инструмент, то ничего больше сделать не можем

                int orderID = 0; // пытаемся сопоставить заявку
                if (_order_rid_lid.ContainsKey(rtrd.OrderID)) orderID = _order_rid_lid[rtrd.OrderID];
                if (orderID == 0) continue; // если не смогли сопоставить заявку, то ничего больше сделать не можем

                if (_trade_rid_lid.ContainsKey(rtrd.TradeID))
                {
                    int lid = _trade_rid_lid[rtrd.TradeID];
                    var trade = trades.FirstOrDefault(r => r.TradeID == lid);
                    if (trade == null) // локальный не найден
                    {
                        _trade_rid_lid.Remove(rtrd.TradeID); // удаляем старое соответствие
                        var trd = _accountDA.CreateTrade(localAccountID, orderID, rtrd.Time, insID, rtrd.BuySell, rtrd.LotCount, rtrd.Price, rtrd.Comm, rtrd.TradeNo);
                        _trade_rid_lid.Add(rtrd.TradeID, trd.TradeID);
                    }
                }
                else // соответствие не найдено
                {
                    var trd = _accountDA.CreateTrade(localAccountID, orderID, rtrd.Time, insID, rtrd.BuySell, rtrd.LotCount, rtrd.Price, rtrd.Comm, rtrd.TradeNo);
                    _trade_rid_lid.Add(rtrd.TradeID, trd.TradeID);
                }
            }
        }

        /// <summary>
        /// Синхронизация позиций по бумагам
        /// </summary>
        /// <param name="localAccountID">Локальный AccountID</param>
        /// <param name="remoteAccountID">Удаленный AccountID</param>
        private void SyncHoldings(int localAccountID, int remoteAccountID)
        {
            var remHoldings = _leechDA.GetHoldingList(remoteAccountID);
            var holdings = _accountDA.GetHoldings(localAccountID);

            foreach (var r_hold in remHoldings)
            {
                if (!_instrum_rid_lid.ContainsKey(r_hold.InsID)) continue;

                int l_insID = _instrum_rid_lid[r_hold.InsID];
                var l_hold = holdings.FirstOrDefault(r => r.InsID == l_insID);
                if (l_hold == null)
                {
                    _accountDA.CreateHolding(localAccountID, l_insID, r_hold.LotCount);
                }
                else if (l_hold.LotCount != r_hold.LotCount)
                {
                    l_hold.LotCount = r_hold.LotCount;
                    _accountDA.UpdateHolding(l_hold);
                }
            }

            foreach (var l_hold in holdings)
            {
                if (!_instrum_rid_lid.ContainsValue(l_hold.InsID)) continue;

                var r_insID = _instrum_rid_lid.FirstOrDefault(r => r.Value == l_hold.InsID).Key;
                if (remHoldings.FirstOrDefault(r => r.InsID == r_insID) == null) // в локальной базе есть запись с инструментом, а в удаленной базе нет такого инструмента, значит удаляем
                {
                    _accountDA.DeleteHolding(l_hold.HoldingID);
                }
            }
        }

        /// <summary>
        /// Синхронизация позиций по деньгам
        /// </summary>
        /// <param name="localAccountID">Локальный AccountID</param>
        /// <param name="remoteAccountID">Удаленный AccountID</param>
        private void SyncCash(int localAccountID, int remoteAccountID)
        {
            var remCash = _leechDA.GetCash(remoteAccountID);
            if (remCash == null) return;

            var cash = _accountDA.GetCash(localAccountID);

            if (cash == null)
            {
                _accountDA.CreateCash(localAccountID, remCash.Initial, remCash.Current,
                    remCash.Sell, remCash.Buy, remCash.SellComm, remCash.BuyComm);
            }
            else
            {
                bool isUpdate = false;
                if (cash.Buy != remCash.Buy) { cash.Buy = remCash.Buy; isUpdate = true; }
                if (cash.BuyComm != remCash.BuyComm) { cash.BuyComm = remCash.BuyComm; isUpdate = true; }
                if (cash.Sell != remCash.Sell) { cash.Sell = remCash.Sell; isUpdate = true; }
                if (cash.SellComm != remCash.SellComm) { cash.SellComm = remCash.SellComm; isUpdate = true; }
                if (cash.Current != remCash.Current) { cash.Current = remCash.Current; isUpdate = true; }
                if (cash.Initial != remCash.Initial) { cash.Initial = remCash.Initial; isUpdate = true; }

                if (isUpdate) _accountDA.UpdateCash(cash);
            }
        }

        public async Task SyncAllTradesAsync()
        {
            var dbPath = _config.GetLeechDataPath();
            var dateDirs = Directory.GetDirectories(dbPath);

            foreach (var dateDir in dateDirs)
            {
                int year, month, day;
                var ymd = Path.GetFileName(dateDir).Split('-');
                if (ymd.Length < 3) continue;
                if (!int.TryParse(ymd[0], out year)) continue;
                if (!int.TryParse(ymd[1], out month)) continue;
                if (!int.TryParse(ymd[2], out day)) continue;
                if (year < 1900 || year > 2999) continue;
                if (month < 1 || month > 12) continue;
                if (day < 1 || day > 31) continue;

                var allTradesPath = Path.Combine(dateDir, "AllTrades");
                if (!Directory.Exists(allTradesPath)) continue; // нет каталога AllTrades

                var date = new DateTime(year, month, day);
                var instrumIDs = _tickHistoryDA.GetInstrums(date);
                var tickers = Directory.GetFiles(allTradesPath).Select(f => Path.GetFileName(f)).ToList();

                foreach (var ticker in tickers)
                {
                    var instrum = _instrumDA.GetInstrum(0, ticker);
                    if (instrum == null) continue; // не найден тикер
                    if (instrumIDs.Contains(instrum.InsID)) continue; // данные уже есть на нужную дату и тикер

                    var bytes = await File.ReadAllBytesAsync(Path.Combine(allTradesPath, ticker));
                    _tickHistoryDA.InsertData(instrum.InsID, date, bytes);
                }
            }
        }
    }
}
