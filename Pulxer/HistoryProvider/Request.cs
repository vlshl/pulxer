using Common.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace Pulxer.HistoryProvider
{
    public class Requester : IRequester
    {
        private ILogger<Requester> _logger;

        public Requester(ILogger<Requester> logger)
        {
            _logger = logger;
        }

        public byte[] Request(string url)
        {
            _logger.LogInformation("RequestAsync: " + url);

            MemoryStream ms = new MemoryStream();
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                WebResponse resp = req.GetResponseAsync().Result;
                Stream s = resp.GetResponseStream();
                s.CopyTo(ms);
                _logger.LogInformation("GetResponse: " + ms.Length + " bytes");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Request error");
                throw new Exception("Ошибка при загрузке данных. Url='" + url + "'", ex);
            }

            return ms.ToArray();
        }
    }

    //public class RequesterTest : IRequester
    //{
    //    private readonly IConsole _console = null;

    //    public RequesterTest(IConsole console)
    //    {
    //        _console = console;
    //    }

    //    public async Task<byte[]> RequestAsync(string url)
    //    {
    //        _console.WriteLine(url);

    //        await Task.Factory.StartNew(() => { Thread.Sleep(5000); });

    //        return new byte[] { };
    //    }
    //}
}
