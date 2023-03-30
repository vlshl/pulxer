using Common.Data;
using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pulxer
{
    public class InstrumCache
    {
        private Dictionary<int, Instrum> _insId_instrum;
        private readonly IServiceProvider _sp;

        public InstrumCache(IServiceProvider sp)
        {
            _sp = sp;
            Initialize();
        }

        public void Initialize()
        {
            using (var scope = _sp.CreateScope())
            {
                var instrumDA = scope.ServiceProvider.GetService<IInstrumDA>();
                var list = instrumDA.GetInstrums();
                _insId_instrum = list.ToDictionary(r => r.InsID, r => r);
            }
        }

        public Instrum GetById(int insId)
        {
            if (_insId_instrum.ContainsKey(insId)) return _insId_instrum[insId];
            return null;
        }
    }
}
