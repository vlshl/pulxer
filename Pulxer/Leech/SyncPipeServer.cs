using Common.Data;
using LeechPipe;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Interfaces;
using Platform;

namespace Pulxer.Leech
{
    public class SyncPipeServer : ISyncPipeServer
    {
        private ILpCore _core;
        private ushort _pipe;

        public SyncPipeServer(ILpCore core, ushort pipe)
        {
            _core = core;
            _pipe = pipe;
        }

        public async Task<Instrum[]> GetInstrumList()
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetInstrumList"));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<Instrum[]>(data);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Account[]> GetAccountList()
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetAccountList"));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<Account[]>(data);
            }
            catch
            {
                return null;
            }
        }

        public async Task<StopOrder[]> GetStopOrderList(int accountId)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetStopOrderList " + accountId.ToString()));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<StopOrder[]>(data);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Order[]> GetOrderList(int accountId)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetOrderList " + accountId.ToString()));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<Order[]>(data);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Trade[]> GetTradeList(int accountId)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetTradeList " + accountId.ToString()));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<Trade[]>(data);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Cash> GetCash(int accountId)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetCash " + accountId.ToString()));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<Cash>(data);
            }
            catch
            {
                return null;
            }
        }

        public async Task<Holding[]> GetHoldingList(int accountId)
        {
            var res = await _core.SendMessageAsync(_pipe, Encoding.UTF8.GetBytes("GetHoldingList " + accountId.ToString()));
            if (res == null) return null;

            try
            {
                var data = Encoding.UTF8.GetString(res);
                return JsonConvert.DeserializeObject<Holding[]>(data);
            }
            catch
            {
                return null;
            }
        }
    }
}
