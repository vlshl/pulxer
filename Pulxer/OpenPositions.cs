using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pulxer.Leech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Data;
using Common;

namespace Pulxer
{
    public class OpenPositions
    {
        private readonly ITgContextManager _ctxMgr;
        private readonly LeechServerManager _lsm;
        private readonly IServiceProvider _serviceProvider;
        private bool _isWorking;
        private List<OpenPosItem> _items;
        private readonly ILogger<OpenPositions> _logger;
        private int SLEEP_10 = 10000;
        private int SLEEP_60 = 60 * 1000;
        private int PULSE_TIMEOUT = 10 * 1000;
        private readonly OpenPosTgContext _ctx;

        public OpenPositions(ITgContextManager ctxMgr, LeechServerManager lsm, IServiceProvider services, ILogger<OpenPositions> logger)
        {
            _ctxMgr = ctxMgr;
            _lsm = lsm;
            _serviceProvider = services;
            _logger = logger;
            _items = new List<OpenPosItem>();
            _isWorking = false;
            _ctx = new OpenPosTgContext(this, ctxMgr);
        }

        public void Start()
        {
            if (_isWorking) return;

            _isWorking = true;
            RefreshItems();
            _ctxMgr.RegisterContext(_ctx);
            Task.Run(DoWork);
        }

        public void Stop()
        {
            _isWorking = false;
            _ctxMgr.UnregisterContext(_ctx);
        }

        private void DoWorkTest()
        {
            var posList = GetPositions();

            while (_isWorking)
            {
                foreach (var pos in posList)
                {
                    decimal cp = pos.CurPrice;
                    if (cp == 0)
                        pos.SetCurPrice(pos.OpenPrice);
                    else
                        pos.SetCurPrice(cp * 1.001m);
                }
                _ctx.Pulse(posList).Wait();
                Thread.Sleep(SLEEP_10); // sleep 10 sec
            }
        }

        private void DoWork()
        {
            TickPipeServer tps = null;
            LeechServer ls = null;

            while (_isWorking && (tps == null))
            {
                ls = _lsm.GetServer();
                if (ls == null)
                {
                    _logger.LogWarning("LeechServer does not exist");
                    Thread.Sleep(SLEEP_60);
                    continue;
                }

                tps = ls.CreateTickPipe().Result;
                if (tps == null)
                {
                    _logger.LogWarning("TickPipeServer creation error");
                    Thread.Sleep(SLEEP_60);
                    continue;
                }

                _logger.LogInformation("TickPipeServer is ready");
            }

            while (_isWorking)
            {
                string[] tickers;
                lock (_items)
                {
                    tickers = _items.Select(r => r.Ticker).Distinct().ToArray();
                }

                if (tickers.Any())
                {
                    var prices = tps.GetLastPrices(tickers).Result;

                    lock (_items)
                    {
                        foreach (var item in _items)
                        {
                            var lastPrice = prices.FirstOrDefault(r => r.Ticker == item.Ticker);
                            if (lastPrice.Ticker == "") continue;

                            item.SetCurPrice(lastPrice.Price);
                        }
                    }

                    bool isSuccess = _ctx.Pulse(GetPositions()).Wait(PULSE_TIMEOUT);
                    if (!isSuccess)
                    {
                        _logger.LogWarning("OpenPosTgContext:Pulse timeout");
                    }
                }

                Thread.Sleep(SLEEP_10); // sleep 10 sec
            }

            ls.DeletePipe(tps.GetPipe()).Wait();
        }

        public void RefreshItems()
        {
            List<OpenPosItem> items = new List<OpenPosItem>();

            using (var scope = _serviceProvider.CreateScope())
            {
                var accountBL = scope.ServiceProvider.GetService<IAccountBL>();
                var positionBL = scope.ServiceProvider.GetService<IPositionBL>();
                var instrumBL = scope.ServiceProvider.GetService<IInstrumBL>();

                var acc = accountBL.GetRealAccount();
                if (acc != null)
                {
                    var posList = positionBL.GetOpenedPositions(acc.AccountID);
                    foreach (var pos in posList)
                    {
                        var instrum = instrumBL.GetInstrumByID(pos.InsID);
                        if (instrum == null) continue;
                        items.Add(new OpenPosItem(pos, instrum));
                    }
                }
            }

            lock (_items)
            {
                _items.Clear();
                _items.AddRange(items);
            }
        }

        public OpenPosItem[] GetPositions()
        {
            lock (_items)
            {
                return _items.ToArray();
            }
        }
    }
}
