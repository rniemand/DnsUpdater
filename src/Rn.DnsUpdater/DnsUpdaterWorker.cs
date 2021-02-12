using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Services;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater
{
  public class DnsUpdaterWorker : BackgroundService
  {
    private readonly ILoggerAdapter<DnsUpdaterWorker> _logger;
    private readonly IIpResolverService _resolverService;
    private readonly IDnsUpdaterConfigService _configService;

    public DnsUpdaterWorker(
      ILoggerAdapter<DnsUpdaterWorker> logger,
      IIpResolverService resolverService,
      IDnsUpdaterConfigService configService)
    {
      _logger = logger;
      _resolverService = resolverService;
      _configService = configService;



      _logger.Info("Count of entries: {c}", _configService.DnsEntriesConfig.Entries.Length);

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var rawIpAddress = await _resolverService.GetIpAddress(stoppingToken);

      while (!stoppingToken.IsCancellationRequested)
      {
        _logger.Info(
          "Worker running at: {time} (my IP Address: {ip})",
          DateTimeOffset.Now,
          rawIpAddress
        );

        await Task.Delay(5000, stoppingToken);
      }
    }
  }
}
