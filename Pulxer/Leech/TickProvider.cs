using Common.Data;
using Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer.Leech
{
    public class TickProvider
    {
        private readonly LeechServerManager _lsm;
        private Dictionary<int, int> _insId_count; // инструмент - кол-во подписок на него
        private bool _isWorking;
        private TickDispatcher _tickDisp;
        private int SLEEP_MS = 1000;
        private int SLEEP1_MS = 60 * 1000;
        private ILogger<TickProvider> _logger;

        public TickProvider(LeechServerManager lsm, TickDispatcher td, ILogger<TickProvider> logger)
        {
            _lsm = lsm;
            _tickDisp = td;
            _insId_count = new Dictionary<int, int>();
            _isWorking = false;
            _logger = logger;
            td.SetTickProvider(this);
        }

        public void Open()
        {
            _logger.LogInformation("Open TickProvider");

            _isWorking = true;
            
            Task.Run(async () => 
            {
                TickPipeServer tps = null;
                LeechServer ls = null;

                while(_isWorking && (tps == null))
                {
                    ls = _lsm.GetServer();
                    if (ls == null)
                    {
                        _logger.LogWarning("LeechServer does not exist");
                        Thread.Sleep(SLEEP1_MS);
                        continue;
                    }

                    tps = ls.CreateTickPipe().Result;
                    if (tps == null)
                    {
                        _logger.LogWarning("TickPipeServer creation error");
                        Thread.Sleep(SLEEP1_MS);
                        continue;
                    }

                    _logger.LogInformation("TickPipeServer is ready");
                }

                while (_isWorking)
                {
                    List<int> insIds;
                    lock(_insId_count)
                    {
                        insIds = _insId_count.Keys.ToList();
                    }

                    foreach (var insId in insIds)
                    {
                        int skip = _tickDisp.GetTicksCount(insId);
                        var ticks = await tps.GetLastTicks(insId, skip);
                        if (ticks != null && ticks.Any())
                        {
                            _tickDisp.AddTicks(ticks);
                        }
                    }
                    Thread.Sleep(SLEEP_MS);
                }

                if ((ls != null) && (tps != null))
                {
                    await ls.DeletePipe(tps.GetPipe());
                }

                _logger.LogInformation("Stop TickProvider");
            });
        }

        public void Close()
        {
            _isWorking = false;
            _logger.LogInformation("Close TickProvider");
        }

        public bool AddInstrum(int insId)
        {
            lock (_insId_count)
            {
                if (_insId_count.ContainsKey(insId))
                {
                    _insId_count[insId]++;
                }
                else
                {
                    _insId_count.Add(insId, 1);
                }

                return true;
            }
        }

        public bool RemoveInstrum(int insId)
        {
            lock (_insId_count)
            {
                if ( _insId_count.ContainsKey(insId))
                {
                    _insId_count[insId]--;
                    if (_insId_count[insId] <= 0)
                    {
                        _insId_count.Remove(insId);
                    }
                }

                return true;
            }
        }
    }
}
