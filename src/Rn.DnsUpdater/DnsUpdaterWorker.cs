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
    private readonly IDnsUpdaterService _dnsUpdater;

    public DnsUpdaterWorker(
      ILoggerAdapter<DnsUpdaterWorker> logger,
      IIpResolverService resolverService,
      IDnsUpdaterConfigService configService,
      IDnsUpdaterService dnsUpdater)
    {
      _logger = logger;
      _resolverService = resolverService;
      _configService = configService;
      _dnsUpdater = dnsUpdater;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (DnsUpdaterWorker.ExecuteAsync) Add tests
      while (!stoppingToken.IsCancellationRequested)
      {
        // var rawIpAddress = await _resolverService.GetIpAddress(stoppingToken);
        var rawIpAddress = ":P";
        var dnsEntries = _configService.GetEntriesNeedingUpdate();

        foreach (var dnsEntry in dnsEntries)
        {
          await _dnsUpdater.UpdateDnsEntry(dnsEntry);
        }

        _logger.Info(
          "Worker running at: {time} (my IP Address: {ip})",
          DateTimeOffset.Now,
          rawIpAddress
        );

        await Task.Delay(_configService.CoreConfig.TickInterval, stoppingToken);
      }
    }
  }
}
