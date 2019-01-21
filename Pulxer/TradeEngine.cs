using Common;
using Common.Data;
using Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using Platform;

namespace BL
{
    public delegate void EngineTradeEH(DateTime time, int insID, BuySell bs, int lotCount, decimal price);
    public delegate void EngineHoldingChangeEH(int insID, int lotCount);
    public delegate void EngineOrderChangeEH(Order order);
    public delegate void EngineStopOrderChangeEH(StopOrder so);

    /// <summary>
    /// Торговый движок. 
    /// Принимает информацию о ценах всех сделок и изменяет состояние заявок, стоп-заявок, создает записи о сделках.
    /// </summary>
    public class TradeEngine
    {
        public event EngineTradeEH OnTrade;
        public event EngineHoldingChangeEH OnHoldingChange;
        public event EngineOrderChangeEH OnOrderChange;
        public event EngineStopOrderChangeEH OnStopOrderChange;
        public bool IsForceSavingChanges = false;

        private TradeEngineData _data = null;
        private bool _isChangeData = false;
        private Dictionary<int, int> _insID_lotSize = new Dictionary<int, int>();
        private readonly IInstrumBL _instrumBL = null;
        private readonly ITimeProvider _timeProvider = null;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="data">Данные по заявкам, сделкам</param>
        /// <param name="instrumBL">Подсистема фин. инструментов</param>
        /// <param name="timeProvider">Провайдер времени</param>
        public TradeEngine(TradeEngineData data, IInstrumBL instrumBL, ITimeProvider timeProvider)
        {
            _data = data;
            _instrumBL = instrumBL;
            _timeProvider = timeProvider;
        }

        /// <summary>
        /// Информирует движок о последней цене фин. инструментов
        /// </summary>
        /// <param name="tick">Тиковые данные</param>
        public void OnTick(Tick tick)
        {
            ProcessTick(tick.Time, tick.InsID, tick.Price, tick.Lots, tick.TradeNo);
            SaveChangedData();
        }

        private void ProcessTick(DateTime time, int insID, decimal price, int lots, long tradeNo)
        {
            // обрабатываем стопы
            IEnumerable<StopOrder> stopOrders = _data.GetActiveStopOrders(insID).ToList();
            foreach (StopOrder stopOrder in stopOrders)
            {
                // снятие просроченных стоп-заявок
                if (stopOrder.EndTime != null && stopOrder.EndTime > time)
                {
                    stopOrder.Status = StopOrderStatus.EndTime;
                    stopOrder.CompleteTime = time;
                    _data.MarkAsModified(stopOrder);
                    RaiseOnStopOrderChangeEvent(stopOrder);
                    _isChangeData = true;
                    continue;
                }

                // срабатывание стоп-заявки
                bool isOrder = false;
                if (stopOrder.StopType == StopOrderType.StopLoss)
                {
                    if (stopOrder.BuySell == BuySell.Buy)
                    {
                        isOrder = price >= stopOrder.AlertPrice;
                    }
                    else
                    {
                        isOrder = price <= stopOrder.AlertPrice;
                    }
                }
                else if (stopOrder.StopType == StopOrderType.TakeProfit)
                {
                    if (stopOrder.BuySell == BuySell.Buy)
                    {
                        isOrder = price <= stopOrder.AlertPrice;
                    }
                    else
                    {
                        isOrder = price >= stopOrder.AlertPrice;
                    }
                }

                if (isOrder)
                {
                    Order order = _data.AddOrder(time, stopOrder.InsID, (BuySell)stopOrder.BuySell,
                        stopOrder.LotCount, stopOrder.Price, OrderStatus.Active, stopOrder.StopOrderID);
                    stopOrder.Status = StopOrderStatus.Order;
                    stopOrder.CompleteTime = time;
                    _data.MarkAsModified(stopOrder);
                    RaiseOnStopOrderChangeEvent(stopOrder);
                    _isChangeData = true;
                }
            }

            // обрабатываем заявки
            IEnumerable<Order> orders = _data.GetActiveOrders(insID).ToList();
            foreach (Order order in orders)
            {
                // снятие просроченных заявок
                if (order.Time.Date < time.Date)
                {
                    order.Status = OrderStatus.EndTime;
                    _data.MarkAsModified(order);
                    RaiseOnOrderChangeEvent(order);
                    _isChangeData = true;
                    continue;
                }

                // совершение сделок
                if (order.Price == null)
                {
                    AddTrade(order, time, price, tradeNo);
                }
                else
                {
                    if (order.BuySell == (int)BuySell.Buy && price <= order.Price)
                    {
                        AddTrade(order, time, price, tradeNo); //по какой цене надо совершать сделку?
                    }
                    else if (order.BuySell == BuySell.Sell && price >= order.Price)
                    {
                        AddTrade(order, time, price, tradeNo); //по какой цене надо совершать сделку?
                    }
                }
            }
        }

        private void AddTrade(Order order, DateTime time, decimal price, long tradeNo)
        {
            Cash cash = _data.GetCash();
            Account account = _data.GetAccount();

            decimal summa = price * order.LotCount * GetLotSize(order.InsID);
            decimal commission_summa = summa * account.CommPerc / 100;
            decimal total_summa = summa + commission_summa;

            Holding holding = _data.GetHolding(order.InsID);
            if (order.BuySell == BuySell.Buy)
            {
                if (total_summa > cash.Current)
                {
                    order.Status = OrderStatus.Reject;
                    _data.MarkAsModified(order);
                    RaiseOnOrderChangeEvent(order);
                    _isChangeData = true;
                    return;
                }
            }
            else
            {
                if (!account.IsShortEnable && (holding == null || holding.LotCount < order.LotCount))
                {
                    order.Status = OrderStatus.Reject;
                    _data.MarkAsModified(order);
                    RaiseOnOrderChangeEvent(order);
                    _isChangeData = true;
                    return;
                }
            }

            int oldHold = 0; if (holding != null) oldHold = holding.LotCount;

            int tradeID = _data.AddTrade(order.OrderID, time, order.InsID, order.BuySell, order.LotCount, price,
                commission_summa, tradeNo);
            order.Status = OrderStatus.Trade;
            _data.MarkAsModified(order);
            RaiseOnOrderChangeEvent(order);

            if (order.BuySell == BuySell.Buy)
            {
                if (holding == null)
                {
                    holding = _data.AddHolding(order);
                }
                else
                {
                    holding.LotCount += order.LotCount;
                }

                cash.Current -= total_summa;
                cash.Buy += summa;
                cash.BuyComm += commission_summa;
            }
            else
            {
                if (holding == null)
                {
                    holding = _data.AddHolding(order, true);
                }
                else
                {
                    holding.LotCount -= order.LotCount;
                }

                cash.Current += (summa - commission_summa);
                cash.Sell += summa;
                cash.SellComm += commission_summa;
            }

            _isChangeData = true;

            RaiseOnTradeEvent(time, order.InsID, (BuySell)order.BuySell, order.LotCount, price);

            int newHold = 0; if (holding != null) newHold = holding.LotCount;
            if (oldHold != newHold) RaiseOnHoldingChangeEvent(order.InsID, newHold);
        }

        /// <summary>
        /// Создать новый стоп-ордер.
        /// При срабатывании будет создана заявка.
        /// </summary>
        /// <param name="insID">Фин. инструмент</param>
        /// <param name="bs">Покупка или продажа</param>
        /// <param name="sot">Тип стоп-ордера</param>
        /// <param name="endTime">Время окончания стоп-ордера (при достижении стоп-ордер снимается)</param>
        /// <param name="alertPrice">Сигнальная цена</param>
        /// <param name="price">Цена в заявке при срабатывании стоп-ордера (Null - рыночная цена)</param>
        /// <param name="lotCount">Кол-во лотов в заявке</param>
        /// <returns></returns>
        public StopOrder AddStopOrder(int insID, BuySell bs, StopOrderType sot,
            DateTime? endTime, decimal alertPrice, decimal? price, int lotCount)
        {
            DateTime? time = _timeProvider.CurrentTime;
            if (time == null) return null;

            StopOrder so = _data.AddStopOrder(time.Value, insID, bs, sot, endTime, alertPrice, price, 
                lotCount, StopOrderStatus.Active);
            RaiseOnStopOrderChangeEvent(so);
            _isChangeData = true;
            SaveChangedData();

            return so;
        }

        /// <summary>
        /// У стоп-ордера выставляется статус Remove
        /// </summary>
        /// <param name="so">Стоп-ордер</param>
        public void RemoveStopOrder(StopOrder so)
        {
            DateTime? time = _timeProvider.CurrentTime;
            if (time == null) return;

            so.Status = StopOrderStatus.Remove;
            so.CompleteTime = time.Value;
            _data.MarkAsModified(so);
            RaiseOnStopOrderChangeEvent(so);
            _isChangeData = true;
            SaveChangedData();
        }

        /// <summary>
        /// Изменить стоп-ордер (старый становится Remove и создается новый Active)
        /// </summary>
        /// <param name="so">Старый стоп-ордер</param>
        /// <returns>Новый стоп-ордер</returns>
        public StopOrder ModifyStopOrder(StopOrder so)
        {
            DateTime? time = _timeProvider.CurrentTime;
            if (time == null) return null;

            so.Status = StopOrderStatus.Remove;
            so.CompleteTime = time.Value;
            _data.MarkAsModified(so);
            RaiseOnStopOrderChangeEvent(so);

            StopOrder so1 = _data.AddStopOrder(time.Value, so.InsID, so.BuySell, so.StopType, so.EndTime, so.AlertPrice, so.Price, so.LotCount, StopOrderStatus.Active);
            RaiseOnStopOrderChangeEvent(so1);
            _isChangeData = true;
            SaveChangedData();

            return so1;
        }

        /// <summary>
        /// Удаление активных стоп-ордеров (Active -> Remove)
        /// </summary>
        /// <param name="insID">Фин. инструмент</param>
        /// <param name="soType">Удаляемые ордера SL/TP (null-все)</param>
        /// <param name="bs">Удаляемые ордера Buy/Sell (null-все)</param>
        public void RemoveActiveStopOrders(int insID, StopOrderType? soType = null, BuySell? bs = null)
        {
            DateTime? time = _timeProvider.CurrentTime;
            if (time == null) return;

            var stopOrders = _data.GetActiveStopOrders(insID, soType, bs);
            foreach (StopOrder so in stopOrders)
            {
                if (soType != null && so.StopType != soType.Value) continue;

                so.Status = StopOrderStatus.Remove;
                so.CompleteTime = time.Value;
                _data.MarkAsModified(so);
                RaiseOnStopOrderChangeEvent(so);
                _isChangeData = true;
            }
            SaveChangedData();
        }

        /// <summary>
        /// Получить список активных стоп-ордеров
        /// </summary>
        /// <param name="insID">Фин. инструмент</param>
        /// <param name="soType">Тип стоп-ордера (null-все)</param>
        /// <param name="bs">Покупка или продажа (null-все)</param>
        /// <returns></returns>
        public IEnumerable<StopOrder> GetActiveStopOrders(int insID, StopOrderType? soType = null, BuySell? bs = null)
        {
            return _data.GetActiveStopOrders(insID, soType, bs);
        }

        /// <summary>
        /// Создать заявку.
        /// Время жизни заявки - одна торговая сессия
        /// </summary>
        /// <param name="insID">Фин. инструмент</param>
        /// <param name="bs">Покупка или продажа</param>
        /// <param name="price">Цена (null-рыночная)</param>
        /// <param name="lotCount">Кол-во лотов</param>
        /// <returns>Новая заявка</returns>
        public Order AddOrder(int insID, BuySell bs, decimal? price, int lotCount)
        {
            DateTime? time = _timeProvider.CurrentTime;
            if (time == null) return null;

            Order order = _data.AddOrder(time.Value, insID, bs, lotCount, price, OrderStatus.Active, null);
            RaiseOnOrderChangeEvent(order);
            _isChangeData = true;
            SaveChangedData();

            return order;
        }

        /// <summary>
        /// У заявки выставляется статус Remove
        /// </summary>
        /// <param name="order">Заявка</param>
        public void RemoveOrder(Order order)
        {
            DateTime? time = _timeProvider.CurrentTime;
            if (time == null) return;

            order.Status = OrderStatus.Remove;
            _data.MarkAsModified(order);
            RaiseOnOrderChangeEvent(order);
            _isChangeData = true;
            SaveChangedData();
        }


        /// <summary>
        /// Изменить заявку (старая становится Remove и создается новая)
        /// </summary>
        /// <param name="order">Старая заявка</param>
        /// <returns>Новая заявка</returns>
        public Order ModifyOrder(Order order)
        {
            DateTime? time = _timeProvider.CurrentTime;
            if (time == null) return null;

            order.Status = OrderStatus.Remove;
            _data.MarkAsModified(order);
            RaiseOnOrderChangeEvent(order);

            Order order1 = _data.AddOrder(time.Value, order.InsID, order.BuySell, order.LotCount, order.Price, OrderStatus.Active, null);
            RaiseOnOrderChangeEvent(order1);
            _isChangeData = true;
            SaveChangedData();

            return order1;
        }

        /// <summary>
        /// Количество лотов в портфеле по данному инструменту
        /// </summary>
        /// <param name="insID">Фин. инструмент</param>
        /// <returns>Кол-во лотов. Положительное значение - позиция long, отрицательное значение - позиция short, 0 - инструмента нет в портфеле.</returns>
        public int GetHoldingLots(int insID)
        {
            Holding h = _data.GetHolding(insID);
            return h != null ? h.LotCount : 0;
        }

        /// <summary>
        /// Принудительное закрытие позиции по последнему тику (т.е. с ценой и датой указанного тика)
        /// Текущее время также устанавливается по времени тика.
        /// Создается заявка и по этой заявке тут же создается сделка.
        /// Если позиция long - происходит продажа, если short - покупка.
        /// </summary>
        /// <param name="lastTick">Ценовые данные, по которым происходит закрыние позиции.</param>
        public void ClosePosition(Tick lastTick)
        {
            int lots = GetHoldingLots(lastTick.InsID);
            if (lots == 0) return;

            Order order = AddOrder(lastTick.InsID, 
                lots > 0 ? BuySell.Sell : BuySell.Buy,
                lastTick.Price, Math.Abs(lots));
            AddTrade(order, lastTick.Time, lastTick.Price, lastTick.TradeNo);
        }

        private void RaiseOnTradeEvent(DateTime time, int insID, BuySell bs, int lotCount, decimal price)
        {
            if (OnTrade != null) OnTrade(time, insID, bs, lotCount, price);
        }

        private void RaiseOnHoldingChangeEvent(int insID, int lotCount)
        {
            if (OnHoldingChange != null) OnHoldingChange(insID, lotCount);
        }

        private void RaiseOnOrderChangeEvent(Order order)
        {
            if (OnOrderChange != null) OnOrderChange(order);
        }

        private void RaiseOnStopOrderChangeEvent(StopOrder so)
        {
            if (OnStopOrderChange != null) OnStopOrderChange(so);
        }

        private void SaveChangedData()
        {
            if (_isChangeData && IsForceSavingChanges)
            {
                _data.SaveData();
            }
            _isChangeData = false;
        }

        private int GetLotSize(int insID)
        {
            if (_insID_lotSize == null) _insID_lotSize = new Dictionary<int, int>();

            if (_insID_lotSize.ContainsKey(insID))
            {
                return _insID_lotSize[insID];
            }
            else
            {
                Instrum ins = _instrumBL.GetInstrumByID(insID);
                if (ins != null)
                {
                    _insID_lotSize.Add(ins.InsID, ins.LotSize);
                    return ins.LotSize;
                }
                else
                {
                    _insID_lotSize.Add(insID, 1);
                    return 1;
                }
            }
        }
    }
}
