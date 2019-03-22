﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Platform
{
    public delegate void OnTimerDelegate(DateTime? time, int delay);
    public delegate void OnTickDelegate(DateTime time, decimal price, int lots);

    public interface ILeechPlatform
    {
        void AddLog(string source, string text);
        IInstrum GetInstrum(string ticker);
        Task<IBarRow> CreateBarRow(int insID, Timeframes tf, int historyDays);
        void Close();
        void OnTimer(OnTimerDelegate onTimer);
        void OnTick(int insID, OnTickDelegate onTick);

        Order AddBuyOrder(int insID, decimal? price, int lots);
        Order AddSellOrder(int insID, decimal? price, int lots);
        void RemoveOrder(Order order);

        StopOrder LongStopLoss(int insID, decimal alertPrice, decimal? price, int lots);
        StopOrder LongTakeProfit(int insID, decimal alertPrice, decimal? price, int lots);
        StopOrder ShortStopLoss(int insID, decimal alertPrice, decimal? price, int lots);
        StopOrder ShortTakeProfit(int insID, decimal alertPrice, decimal? price, int lots);
        void RemoveStopOrder(StopOrder so);
        IEnumerable<StopOrder> GetStopOrders(int insID);
        void RemoveStopOrders(int insID);
        int GetHolding(int insID);

        decimal GetCommPerc();
        bool GetShortEnable();
        decimal GetInitialSumma();
        decimal GetCurrentSumma();
    }
}
