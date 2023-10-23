using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using Xunit;

namespace Pulxer.Test
{
    public class TickHistoryTest
    {
        private ILogger<TickHistory> _logger;

        public TickHistoryTest() 
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder.AddConsole());
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            _logger = loggerFactory.CreateLogger<TickHistory>();
        }


        [Fact]
        public void SaveTicksBlobAsync_Test()
        {
            var config = new ConfigMock(@"d:\\tmp\\TickHistoryTest1");

            Cleanup(config);

            TickHistory th = new TickHistory(config, _logger);
            bool isSuccess = th.SaveTicksBlobAsync(new DateTime(2020, 1, 5), "TICKER", new byte[] { 1, 2, 3, 4, 5 }).Result;

            Assert.True(isSuccess);

            Cleanup(config);
        }

        [Fact]
        public void GetLastDate_Test()
        {
            var config = new ConfigMock(@"d:\\tmp\\TickHistoryTest2");

            Cleanup(config);

            TickHistory th = new TickHistory(config, _logger);
            var lastDate = th.GetLastDate();

            Assert.Null(lastDate);

            th.SaveTicksBlobAsync(new DateTime(2020, 1, 5), "TICKER1", new byte[] { 1, 2, 3, 4, 5 }).Wait();
            th.SaveTicksBlobAsync(new DateTime(2021, 1, 3), "TICKER2", new byte[] { 1, 2, 3, 4, 5 }).Wait();
            var lastDate1 = th.GetLastDate();

            Assert.NotNull(lastDate1);
            Assert.Equal("2021-01-03", lastDate1.Value.ToString("yyyy-MM-dd"));

            Cleanup(config);
        }

        [Fact]
        public void GetDates_Test()
        {
            var config = new ConfigMock(@"d:\\tmp\\TickHistoryTest3");

            Cleanup(config);

            TickHistory th = new TickHistory(config, _logger);
            var dates = th.GetDates();

            Assert.Empty(dates);

            th.SaveTicksBlobAsync(new DateTime(2020, 1, 5), "TICKER1", new byte[] { 1, 2, 3, 4, 5 }).Wait();
            th.SaveTicksBlobAsync(new DateTime(2021, 1, 3), "TICKER2", new byte[] { 1, 2, 3, 4, 5 }).Wait();
            var dates1 = th.GetDates();

            Assert.Equal(2, dates1.Length);

            var dates2 = th.GetDates(2020);

            Assert.Single(dates2);

            var dates3 = th.GetDates(2018);

            Assert.Empty(dates3);

            Cleanup(config);
        }

        private void Cleanup(IConfig config)
        {
            if (Directory.Exists(config.GetTickHistoryPath()))
            {
                Directory.Delete(config.GetTickHistoryPath(), true);
            }
        }
    }

    internal class ConfigMock : IConfig
    {
        private readonly string _tickHistoryPath;

        public ConfigMock(string tickHistoryPath)
        {
            _tickHistoryPath = tickHistoryPath;
        }

        public string GetBotsPath()
        {
            throw new NotImplementedException();
        }

        public int GetHistoryDownloaderDays(string tf)
        {
            throw new NotImplementedException();
        }

        public int GetHistoryDownloaderDelay(string tf)
        {
            throw new NotImplementedException();
        }

        public int GetHistoryDownloaderMonths(string tf)
        {
            throw new NotImplementedException();
        }

        public string GetHistoryProviderCache()
        {
            throw new NotImplementedException();
        }

        public string GetHistoryProviderConfig()
        {
            throw new NotImplementedException();
        }

        public string GetPluginsPath()
        {
            throw new NotImplementedException();
        }

        public string GetTickHistoryPath()
        {
            return _tickHistoryPath;
        }
    }


}
