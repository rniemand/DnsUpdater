using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;
using Rn.DnsUpdater.Services;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics;
using Rn.NetCore.Metrics.Builders;

namespace Rn.DnsUpdater;

public class DnsUpdaterWorker : BackgroundService
{
  private readonly ILoggerAdapter<DnsUpdaterWorker> _logger; 
  private readonly IHostIpAddressService _addressService;
  private readonly IConfigService _configService;
  private readonly IDnsUpdaterService _dnsUpdater;
  private readonly IHeartbeatService _heartbeatService;
  private readonly IMetricService _metrics;

  public DnsUpdaterWorker(
    ILoggerAdapter<DnsUpdaterWorker> logger,
    IHostIpAddressService addressService,
    IConfigService configService,
    IDnsUpdaterService dnsUpdater,
    IMetricService metrics,
    IHeartbeatService heartbeatService)
  {
    _logger = logger; 
    _addressService = addressService;
    _configService = configService;
    _dnsUpdater = dnsUpdater;
    _metrics = metrics;
    _heartbeatService = heartbeatService;
  }

  // Required methods
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    // TODO: [TESTS] (DnsUpdaterWorker.ExecuteAsync) Add tests
    while (!stoppingToken.IsCancellationRequested)
    {
      await _heartbeatService.TickAsync();
      var hostAddressChanged = await _addressService.HostAddressChangedAsync(stoppingToken);

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

    _logger.LogInformation(dnsEntries.Count == 1
      ? "Updating 1 DNS entry"
      : $"Updating {dnsEntries.Count} DNS entries");

    var builder = new ServiceMetricBuilder(nameof(DnsUpdaterWorker), nameof(UpdateDnsEntries))
      .WithCategory(MetricCategory.DnsUpdater, MetricSubCategory.UpdateEntries)
      .WithCustomInt1(dnsEntries.Count);

    try
    {
      using (builder.WithTiming())
      {
        foreach (var dnsEntry in dnsEntries)
        {
          builder.IncrementQueryCount();
          await _dnsUpdater.UpdateEntryAsync(dnsEntry, stoppingToken);
        }

        _configService.SaveConfigState();
      }
    }
    catch (Exception ex)
    {
      builder.WithException(ex);
      _logger.LogUnexpectedException(ex);
    }
    finally
    {
      await _metrics.SubmitBuilderAsync(builder);
    }
  }
}
