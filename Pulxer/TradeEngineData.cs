using Common;
using Common.Data;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BL
{
    /// <summary>
    /// Интерфейс объекта данных торгового движка
    /// </summary>
    public interface ITradeEngineData
    {
        Account GetAccount();
        Cash GetCash();
        IEnumerable<Holding> GetHoldings();
        IEnumerable<Trade> GetTrades(int insID = 0);
        IEnumerable<Order> GetOrders(int insID = 0);
        IEnumerable<StopOrder> GetStopOrders(int insID = 0);
    }

    /// <summary>
    /// Данные торгового счета (заявки, сделки, стоп-заявки, позиции и т.д.)
    /// </summary>
    public class TradeEngineData : ITradeEngineData
    {
        private Account _account = null;
        private Cash _cash = null;
        private List<Holding> _holdings;
        private List<Order> _orders;
        private List<StopOrder> _stopOrders;
        private List<Trade> _trades;
        private readonly IAccountDA _accountDA;
        private List<Order> _modifiedOrders;
        private List<StopOrder> _modifiedStopOrders;

        public TradeEngineData(IAccountDA accountDA)
        {
            _accountDA = accountDA;

            _account = new Account();
            _cash = new Cash();
            _holdings = new List<Holding>();
            _orders = new List<Order>();
            _stopOrders = new List<StopOrder>();
            _trades = new List<Trade>();

            _modifiedOrders = new List<Order>();
            _modifiedStopOrders = new List<StopOrder>();
        }

        /// <summary>
        /// Список активных заявок
        /// </summary>
        /// <param name="insID">Фин. инструмент</param>
        /// <returns>Список активных заявок</returns>
        public IEnumerable<Order> GetActiveOrders(int insID)
        {
            return _orders.Where(r => r.InsID == insID && r.Status == (byte)OrderStatus.Active);
        }

        /// <summary>
        /// Список активных стоп-ордеров
        /// </summary>
        /// <param name="insID">Фин. инструмент</param>
        /// <returns>Список активных стоп-ордеров</returns>
        public IEnumerable<StopOrder> GetActiveStopOrders(int insID)
        {
            return _stopOrders.Where(r => r.InsID == insID && r.Status == (byte)StopOrderStatus.Active);
        }

        /// <summary>
        /// Список активных стоп-ордеров. Расширенный отбор.
        /// </summary>
        /// <param name="insID">Фин инструмент</param>
        /// <param name="soType">Тип стоп-ордера</param>
        /// <param name="bs">Покупка/продажа</param>
        /// <returns>Список активных стоп-ордеров</returns>
        public IEnumerable<StopOrder> GetActiveStopOrders(int insID, StopOrderType? soType = null, BuySell? bs = null)
        {
            var stopOrders = GetActiveStopOrders(insID);
            if (soType != null) stopOrders = stopOrders.Where(so => so.StopType == soType.Value);
            if (bs != null) stopOrders = stopOrders.Where(so => so.BuySell == bs.Value);

            return stopOrders.ToList();
        }

        /// <summary>
        /// Торговый счет
        /// </summary>
        /// <returns></returns>
        public Account GetAccount()
        {
            return _account;
        }

        /// <summary>
        /// Позиции по деньгам
        /// </summary>
        /// <returns></returns>
        public Cash GetCash()
        {
            return _cash;
        }

        /// <summary>
        /// Создать новую заявку
        /// </summary>
        /// <param name="time">Дата и время создания</param>
        /// <param name="insID">Фин. инструмент</param>
        /// <param name="bs">Покупка или продажа</param>
        /// <param name="lotCount">Кол-во лотов</param>
        /// <param name="price">Цена заявки (null-рыночная цена, т.е. такая заявка тут же выполнится, если есть торги по этому инструменту)</param>
        /// <param name="status">Статус новой заявки</param>
        /// <param name="stopOrderID">Ссылка на стоп-ордер (не обязательно)</param>
        /// <returns>Созданная заявка</returns>
        public Order AddOrder(DateTime time, int insID, BuySell bs, int lotCount, decimal? price,
            OrderStatus status, int? stopOrderID)
        {
            Order order = new Order();

            order.OrderID = GetNewID<Order>(_orders, r => r.OrderID);
            order.Time = time;
            order.InsID = insID;
            order.BuySell = bs;
            order.LotCount = lotCount;
            order.Price = price;
            order.Status = status;
            order.AccountID = _account.AccountID;
            order.StopOrderID = stopOrderID;

            _orders.Add(order);

            return order;
        }

        /// <summary>
        /// Создать новый стоп-ордер
        /// </summary>
        /// <param name="time">Дата и время создания</param>
        /// <param name="insID">Фин инструмент</param>
        /// <param name="bs">Покупка или продажа</param>
        /// <param name="soType">Тип стоп-ордера</param>
        /// <param name="endTime">Время окончания (null-действует бесконечно)</param>
        /// <param name="alertPrice">Цена срабатывания стопа</param>
        /// <param name="price">Цена заявки, которая будет создана при срабатывании стопа (null-заявка по рыночной цене)</param>
        /// <param name="lotCount">Кол-во лотов</param>
        /// <param name="status">Статус нового стоп-ордера</param>
        /// <returns>Новый стоп-ордер</returns>
        public StopOrder AddStopOrder(DateTime time, int insID, BuySell bs, StopOrderType soType, DateTime? endTime,
            decimal alertPrice, decimal? price, int lotCount, StopOrderStatus status)
        {
            StopOrder so = new StopOrder();

            so.StopOrderID = GetNewID<StopOrder>(_stopOrders, r => r.StopOrderID);
            so.Time = time;
            so.InsID = insID;
            so.BuySell = bs;
            so.StopType = soType;
            so.EndTime = endTime;
            so.AlertPrice = alertPrice;
            so.Price = price;
            so.LotCount = lotCount;
            so.Status = status;
            so.AccountID = _account.AccountID;
            so.CompleteTime = null;

            _stopOrders.Add(so);

            return so;
        }

        /// <summary>
        /// Создание новой сделки
        /// </summary>
        /// <param name="orderID">Ссылка на заявку</param>
        /// <param name="time">Дата и время сделки</param>
        /// <param name="insID">Фин инструмент</param>
        /// <param name="bs">Покупка или продажа</param>
        /// <param name="lotCount">Кол-во лотов</param>
        /// <param name="price">Цена</param>
        /// <param name="commission">Комиссия</param>
        /// <param name="tradeNo">Номер сделки во внешней системе</param>
        /// <returns>Идентификатор (если меньше 0, то временный до записи в базу)</returns>
        public int AddTrade(int orderID, DateTime time, int insID, BuySell bs, int lotCount, decimal price, 
            decimal commission, long tradeNo)
        {
            Trade trade = new Trade();

            trade.TradeID = GetNewID<Trade>(_trades, r => r.TradeID);
            trade.OrderID = orderID;
            trade.Time = time;
            trade.InsID = insID;
            trade.BuySell = bs;
            trade.LotCount = lotCount;
            trade.Price = price;
            trade.AccountID = _account.AccountID;
            trade.Comm = commission;
            trade.TradeNo = tradeNo;

            _trades.Add(trade);

            return trade.TradeID;
        }

        /// <summary>
        /// Создает запись Holding с указанным в заявке количеством (isSell=false),
        /// либо с отрицательным количеством (isSell=true)
        /// </summary>
        /// <param name="order">Заявка</param>
        /// <param name="isSell">true-короткая позиция (кол-во берется из заявки с противоположным знаком)</param>
        /// <returns></returns>
        public Holding AddHolding(Order order, bool isSell = false)
        {
            Holding holding = new Holding();

            holding.HoldingID = GetNewID<Holding>(_holdings, r => r.HoldingID);
            holding.InsID = order.InsID;
            holding.LotCount = isSell ? -order.LotCount : order.LotCount;
            holding.AccountID = _account.AccountID;

            _holdings.Add(holding);

            return holding;
        }

        /// <summary>
        /// Позиция по инструменту
        /// </summary>
        /// <param name="insID">Фин инструмент</param>
        /// <returns>Позиция</returns>
        public Holding GetHolding(int insID)
        {
            return _holdings.FirstOrDefault(h => h.InsID == insID);
        }

        /// <summary>
        /// Список всех позиций по инструментам
        /// </summary>
        /// <returns>Список позиций</returns>
        public IEnumerable<Holding> GetHoldings()
        {
            return _holdings;
        }

        /// <summary>
        /// Список сделок (по указанному инструменту или по всем инструментам)
        /// </summary>
        /// <param name="insID">Фин инструмент (0 - по всем инструментам)</param>
        /// <returns>Список сделок</returns>
        public IEnumerable<Trade> GetTrades(int insID = 0)
        {
            if (insID == 0) return _trades.ToArray();
            return _trades.Where(t => t.InsID == insID).ToArray();
        }

        /// <summary>
        /// Список заявок (по указанному инструменту или по всем инструментам)
        /// </summary>
        /// <param name="insID">Фин инструмент</param>
        /// <returns>Список заявок</returns>
        public IEnumerable<Order> GetOrders(int insID = 0)
        {
            if (insID == 0) return _orders.ToArray();
            return _orders.Where(t => t.InsID == insID).ToArray();
        }

        /// <summary>
        /// Список стоп-ордеров по указанному инструменту или по всем инструмнтам
        /// </summary>
        /// <param name="insID">Фин инструмент (0 - по всем инструментам)</param>
        /// <returns>Список стоп-ордеров</returns>
        public IEnumerable<StopOrder> GetStopOrders(int insID = 0)
        {
            if (insID == 0) return _stopOrders.ToArray();
            return _stopOrders.Where(t => t.InsID == insID).ToArray();
        }

        private int GetNewID<T>(List<T> list, Func<T, int> func)
        {
            int id = -1;
            var ids = list.Select(func).Where(i => i < 0).ToList();
            if (ids.Count > 0) id = ids.Min() - 1;

            return id;
        }

        /// <summary>
        /// Пометить заявку как измененную
        /// </summary>
        /// <param name="order">Заявка</param>
        public void MarkAsModified(Order order)
        {
            if (order.OrderID <= 0 || _modifiedOrders.Contains(order)) return;
            _modifiedOrders.Add(order);
        }

        /// <summary>
        /// Пометить стоп-ордер как измененный
        /// </summary>
        /// <param name="stopOrder">Стоп-ордер</param>
        public void MarkAsModified(StopOrder stopOrder)
        {
            if (stopOrder.StopOrderID <= 0 || _modifiedStopOrders.Contains(stopOrder)) return;
            _modifiedStopOrders.Add(stopOrder);
        }

        /// <summary>
        /// Загрузить все данные по торговому счету
        /// </summary>
        /// <param name="accountID">Торговый счет</param>
        public void LoadData(int accountID)
        {
            _account = _accountDA.GetAccountByID(accountID);
            _cash = _accountDA.GetCash(accountID);
            _holdings = _accountDA.GetHoldings(accountID).ToList();
            _orders = _accountDA.GetOrders(accountID).ToList();
            _stopOrders = _accountDA.GetStopOrders(accountID).ToList();
            _trades = _accountDA.GetTrades(accountID).ToList();
        }

        /// <summary>
        /// Сохранить все данные по торговому счету
        /// </summary>
        public void SaveData()
        {
            if (_account.AccountID <= 0)
            {
                _account = _accountDA.CreateAccount(_account.Code, _account.Name, _account.CommPerc, _account.IsShortEnable, _account.AccountType);
                _cash.AccountID = _account.AccountID;
                foreach (Holding holding in _holdings) holding.AccountID = _account.AccountID;
                foreach (Order order in _orders) order.AccountID = _account.AccountID;
                foreach (StopOrder stopOrder in _stopOrders) stopOrder.AccountID = _account.AccountID;
                foreach (Trade trade in _trades) trade.AccountID = _account.AccountID;
            }
            else
            {
                _accountDA.UpdateAccount(_account);
            }

            if (_cash.CashID <= 0)
                _cash = _accountDA.CreateCash(_cash.AccountID, _cash.Initial, _cash.Current, _cash.Sell, _cash.Buy, _cash.SellComm, _cash.BuyComm);
            else
                _accountDA.UpdateCash(_cash);

            foreach (var holding in _holdings)
            {
                if (holding.HoldingID <= 0)
                    _accountDA.CreateHolding(holding.AccountID, holding.InsID, holding.LotCount);
                else
                    _accountDA.UpdateHolding(holding); // holdings всегда обновляем, так проще
            }

            var createStopOrders = _stopOrders.Where(r => r.StopOrderID <= 0).ToList();
            _stopOrders.RemoveAll(r => r.StopOrderID <= 0);
            foreach (var so in _stopOrders)
            {
                if (_modifiedStopOrders.Contains(so)) _accountDA.UpdateStopOrder(so);
            }
            _modifiedStopOrders.Clear();
            foreach (var so in createStopOrders)
            {
                var newSo = _accountDA.CreateStopOrder(so.AccountID, so.Time, so.InsID, so.BuySell, so.StopType, so.EndTime,
                    so.AlertPrice, so.Price, so.LotCount, so.Status, so.CompleteTime, so.StopOrderNo);
                _stopOrders.Add(newSo);
                var orders = _orders.Where(r => r.StopOrderID == so.StopOrderID).ToList();
                foreach (var ord in orders)
                {
                    ord.StopOrderID = newSo.StopOrderID;
                    if (ord.OrderID > 0) MarkAsModified(ord);
                }

            }

            var createOrders = _orders.Where(r => r.OrderID <= 0).ToList();
            _orders.RemoveAll(r => r.OrderID <= 0);
            foreach (var order in _orders)
            {
                if (_modifiedOrders.Contains(order)) _accountDA.UpdateOrder(order);
            }
            _modifiedOrders.Clear();
            foreach (var ord in createOrders)
            {
                var newOrd = _accountDA.CreateOrder(ord.AccountID, ord.Time, ord.InsID, ord.BuySell, ord.LotCount, ord.Price, 
                    ord.Status, ord.StopOrderID, ord.OrderNo);
                _orders.Add(newOrd);
                var trades = _trades.Where(r => r.OrderID == ord.OrderID).ToList();
                foreach (var trd in trades)
                {
                    trd.OrderID = newOrd.OrderID;
                }
            }

            var newTrades = _trades.Where(r => r.TradeID <= 0).ToList();
            _trades.RemoveAll(r => r.TradeID <= 0);
            foreach (var nt in newTrades)
            {
                var t = _accountDA.CreateTrade(nt.AccountID, nt.OrderID, nt.Time, nt.InsID, nt.BuySell, nt.LotCount, nt.Price,
                    nt.Comm, nt.TradeNo);
                _trades.Add(t);
            }
        }

        /// <summary>
        /// Удалить все данные по торговому счету вместе с самим счетом
        /// </summary>
        /// <param name="accountID">Идентификатор торгового счета</param>
        public void DeleteData(int accountID)
        {
            _accountDA.DeleteAccountData(accountID);
        }
    }
}
