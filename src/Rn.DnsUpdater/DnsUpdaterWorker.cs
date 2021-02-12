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
    private readonly IHostIpAddressService _addressService;
    private readonly IDnsUpdaterConfigService _configService;
    private readonly IDnsUpdaterService _dnsUpdater;

    public DnsUpdaterWorker(
      ILoggerAdapter<DnsUpdaterWorker> logger,
      IHostIpAddressService addressService,
      IDnsUpdaterConfigService configService,
      IDnsUpdaterService dnsUpdater)
    {
      _logger = logger;
      _addressService = addressService;
      _configService = configService;
      _dnsUpdater = dnsUpdater;
    }

    // Required methods
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (DnsUpdaterWorker.ExecuteAsync) Add tests
      while (!stoppingToken.IsCancellationRequested)
      {
        var hostAddressChanged = await _addressService.HostAddressChanged(stoppingToken);

        // Decide if we need to update all entries, or just a smaller subset
        var dnsEntries = hostAddressChanged
          ? _configService.GetEnabledEntries()
          : _configService.GetEntriesNeedingUpdate();

        // Update any DnsEntries returned above
        await UpdateDnsEntries(dnsEntries, stoppingToken);

        // Wait for the next loop
        await Task.Delay(_configService.CoreConfig.TickInterval, stoppingToken);
      }
    }

    // Updater methods
    private async Task UpdateDnsEntries(IReadOnlyCollection<DnsUpdaterEntry> dnsEntries, CancellationToken stoppingToken)
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
        await _dnsUpdater.UpdateDnsEntry(dnsEntry, stoppingToken);
      }

      _configService.SaveConfigState();
    }
  }
}
