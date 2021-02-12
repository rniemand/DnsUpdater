using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
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

    // Required methods
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (DnsUpdaterWorker.ExecuteAsync) Add tests
      while (!stoppingToken.IsCancellationRequested)
      {
        // var rawIpAddress = await _resolverService.GetIpAddress(stoppingToken);
        var dnsEntries = _configService.GetEntriesNeedingUpdate();
        
        await UpdateDnsEntries(dnsEntries);

        await Task.Delay(_configService.CoreConfig.TickInterval, stoppingToken);
      }
    }

    // Updater methods
    private async Task UpdateDnsEntries(IReadOnlyCollection<DnsUpdaterEntry> dnsEntries)
    {
      // TODO: [TESTS] (DnsUpdaterWorker.UpdateDnsEntries) Add tests
      // Ensure that we have something to work with
      if (dnsEntries.Count == 0)
        return;

      _logger.Info(dnsEntries.Count == 1
        ? "Updating 1 DNS entry"
        : $"Updating {dnsEntries.Count} DNS entries"
      );

      foreach (var dnsEntry in dnsEntries)
      {
        await _dnsUpdater.UpdateDnsEntry(dnsEntry);
      }

      _configService.SaveConfigState();
    }
  }
}
