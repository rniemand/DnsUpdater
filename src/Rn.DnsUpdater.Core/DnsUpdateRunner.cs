using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Services.Interfaces;
using Rn.NetCore.Common.Logging;

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

  public DnsUpdateRunner(IServiceProvider serviceProvider)
  {
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<DnsUpdateRunner>>();
    _addressService = serviceProvider.GetRequiredService<IHostIpAddressService>();
    _configService = serviceProvider.GetRequiredService<IConfigService>();
    _dnsUpdater = serviceProvider.GetRequiredService<IDnsUpdaterService>();
  }


  public async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    // TODO: [DnsUpdateRunner.ExecuteAsync] (TESTS) Add tests
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
      await Task.Delay(_configService.CoreConfig.TickInterval, stoppingToken);
    }
  }


  private async Task UpdateDnsEntries(IReadOnlyCollection<DnsUpdaterEntry> dnsEntries, CancellationToken stoppingToken)
  {
    // TODO: [TESTS] (DnsUpdaterWorker.UpdateDnsEntries) Add tests
    // Ensure that we have something to work with
    if (dnsEntries.Count == 0)
      return;

    _logger.LogInformation(dnsEntries.Count == 1
      ? "Updating 1 DNS entry"
      : $"Updating {dnsEntries.Count} DNS entries");
    
    foreach (var dnsEntry in dnsEntries)
    {
      await _dnsUpdater.UpdateEntryAsync(dnsEntry, stoppingToken);
    }

    _configService.SaveConfigState();
  }
}
