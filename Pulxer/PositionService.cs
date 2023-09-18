using Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pulxer
{
    public class PositionService : IHostedService
    {
        private readonly ILogger<PositionService> _logger;
        private readonly OpenPositions _openPos;

        public PositionService(IServiceProvider services, OpenPositions openPos, ILogger<PositionService> logger) 
        {
            _openPos = openPos;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Start position service");
            return Task.Run(() => { _openPos.Start();  });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop position service");
            return Task.Run(() => { _openPos.Stop(); });
        }
    }
}
