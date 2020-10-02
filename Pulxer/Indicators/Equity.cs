using Common;
using Common.Interfaces;
using Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pulxer.Indicators
{
    public class Equity
    {
        private ValueRow _cashRow;
        private ValueRow _portfolioRow;
        private ValueRow _equityRow;
        private Dictionary<int, BarRow> _prices;
        private IInstrumBL _instrumBL;
        private IInsStoreBL _insStoreBL;
        private IAccountDA _accountDA;

        public Equity(IInsStoreBL insStoreBL, IInstrumBL instrumBL, IAccountDA accountDA)
        {
            _instrumBL = instrumBL;
            _insStoreBL = insStoreBL;
            _accountDA = accountDA;

            _cashRow = new ValueRow();
            _portfolioRow = new ValueRow();
            _equityRow = new ValueRow();
            _prices = new Dictionary<int, BarRow>();
        }

        public ValueRow CashRow
        {
            get
            {
                return _cashRow;
            }
        }

        public ValueRow PortfolioRow
        {
            get
            {
                return _portfolioRow;
            }
        }

        public ValueRow EquityRow
        {
            get
            {
                return _equityRow;
            }
        }

        public Task Generate(int accountID, Timeline timeline)
        {
            return Task.Run( () => 
            {
                _cashRow.Clear();
                _portfolioRow.Clear();
                _equityRow.Clear();
                _prices.Clear();

                var trades = _accountDA.GetTrades(accountID).OrderBy(r => r.Time).ToList();
                var account = _accountDA.GetAccountByID(accountID);
                var cash = _accountDA.GetCash(accountID);
                List<EqHold> eqHoldList = new List<EqHold>();

                decimal cashSumma = cash.Initial;
                decimal pfSumma = 0;
                var bd1 = timeline.GetBarDate(0);
                var bd2 = timeline.GetBarDate(timeline.Count - 1);
                if (bd1 == null || bd2 == null) return;
                DateTime date1 = bd1.Value.Start.Date;
                DateTime date2 = bd2.Value.NextStart.Date; // для таймфреймов day и week загрузим историю на один лишний день

                int tradeIdx = 0;
                for (int i = 0; i < timeline.Count; i++)
                {
                    var barDates = timeline.GetBarDate(i);
                    if (barDates == null) continue;

                    while (tradeIdx < trades.Count && trades[tradeIdx].Time < barDates.Value.NextStart)
                    {
                        var trade = trades[tradeIdx];
                        var instrum = _instrumBL.GetInstrumByID(trade.InsID); // из кеша
                        var tradeSumma = trade.Price * trade.LotCount * instrum.LotSize;
                        var hold = eqHoldList.FirstOrDefault(r => r.InsID == instrum.InsID);
                        if (trade.BuySell == BuySell.Buy)
                        {
                            cashSumma -= tradeSumma;
                            if (hold != null)
                            {
                                hold.LotCount += trade.LotCount;
                            }
                            else
                            {
                                eqHoldList.Add(new EqHold() { InsID = instrum.InsID, LotSize = instrum.LotSize, LotCount = trade.LotCount });
                            }
                        }
                        else
                        {
                            cashSumma += tradeSumma;
                            if (hold != null)
                            {
                                hold.LotCount -= trade.LotCount;
                                if (hold.LotCount == 0)
                                {
                                    eqHoldList.Remove(hold);
                                }
                            }
                            else
                            {
                                eqHoldList.Add(new EqHold() { InsID = instrum.InsID, LotSize = instrum.LotSize, LotCount = -trade.LotCount });
                            }
                        }

                        tradeIdx++;
                    }

                    // вычисляем сумму портфеля на конец бара
                    pfSumma = 0;
                    foreach (var hold in eqHoldList)
                    {
                        if (!_prices.ContainsKey(hold.InsID))
                        {
                            BarRow barRow = new BarRow(timeline.Timeframe, hold.InsID);
                            _insStoreBL.LoadHistoryAsync(barRow, hold.InsID, date1, date2).Wait();
                            _prices.Add(hold.InsID, barRow);
                        }
                        var bars = _prices[hold.InsID];
                        var bar = bars.Bars.FirstOrDefault(r => r.Time == barDates.Value.Start);
                        if (bar != null)
                        {
                            pfSumma += bar.Close * hold.LotCount * hold.LotSize;
                        }
                    }

                    _cashRow.Add(cashSumma);
                    _portfolioRow.Add(pfSumma);
                    _equityRow.Add(cashSumma + pfSumma);
                }
            });
        }
    }

    internal class EqHold
    {
        public int LotCount { get; set; }
        public int InsID { get; set; }
        public int LotSize { get; set; }
    }
}
