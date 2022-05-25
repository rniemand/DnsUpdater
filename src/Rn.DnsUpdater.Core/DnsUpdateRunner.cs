using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Services;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics;
using Rn.NetCore.Metrics.Builders;

namespace Rn.DnsUpdater.Core;

public interface IDnsUpdateRunner
{
  Task ExecuteAsync(CancellationToken stoppingToken);
}

public class DnsUpdateRunner : IDnsUpdateRunner
{
  private readonly ILoggerAdapter<DnsUpdateRunner> _logger;
  private readonly IHostIpAddressService _addressService;
  private readonly IConfigService _configService;
  private readonly IDnsUpdaterService _dnsUpdater;
  private readonly IMetricService _metrics;

  public DnsUpdateRunner(
    ILoggerAdapter<DnsUpdateRunner> logger,
    IHostIpAddressService addressService,
    IConfigService configService,
    IDnsUpdaterService dnsUpdater,
    IMetricService metrics)
  {
    _logger = logger;
    _addressService = addressService;
    _configService = configService;
    _dnsUpdater = dnsUpdater;
    _metrics = metrics;
  }

  public async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var hostAddressChanged = await _addressService.HostAddressChangedAsync(stoppingToken);

      // Decide if we need to update all entries, or just a smaller subset
      var dnsEntries = hostAddressChanged
        ? _configService.GetEnabledEntries()
        : _configService.GetEntriesNeedingUpdate();

      // Update any DnsEntries returned above
      await UpdateDnsEntries(dnsEntries, stoppingToken);

      // Wait for the next loop
      await _metrics.SubmitBuilderAsync(CreateMetricBuilder("Heartbeat"));
      await Task.Delay(_configService.CoreConfig.TickInterval, stoppingToken);
    }
  }

  private async Task UpdateDnsEntries(IReadOnlyCollection<DnsUpdaterEntry> dnsEntries, CancellationToken stoppingToken)
  {
    // Ensure that we have something to work with
    if (dnsEntries.Count == 0)
      return;
    
    var builder = CreateMetricBuilder(nameof(UpdateDnsEntries));

    try
    {
      _logger.LogInformation(dnsEntries.Count == 1
        ? "Updating 1 DNS entry"
        : $"Updating {dnsEntries.Count} DNS entries");

      builder.WithCustomInt1(dnsEntries.Count);
      using (builder.WithTiming())
      {
        foreach (var dnsEntry in dnsEntries)
        {
          await _dnsUpdater.UpdateEntryAsync(dnsEntry, stoppingToken);
        }
      }

      _configService.SaveConfigState();
    }
    catch (Exception ex)
    {
      builder.WithException(ex);
    }
    finally
    {
      await _metrics.SubmitBuilderAsync(builder);
    }
  }
  
  private static ServiceMetricBuilder CreateMetricBuilder(string method) => new(nameof(DnsUpdateRunner), method);
}
