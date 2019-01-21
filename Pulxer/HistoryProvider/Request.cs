using Common.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer.HistoryProvider
{
    public class Requester : IRequester
    {
        public Requester()
        {
        }

        public async Task<byte[]> RequestAsync(string url)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                WebResponse resp = await req.GetResponseAsync();
                Stream s = resp.GetResponseStream();
                s.CopyTo(ms);
            }
            catch (Exception ex)
            {
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
