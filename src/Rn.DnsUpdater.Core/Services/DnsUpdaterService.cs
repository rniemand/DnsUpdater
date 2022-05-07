using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Enums;
using Rn.DnsUpdater.Core.Services.Interfaces;
using Rn.NetCore.BasicHttp;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Core.Services;

public class DnsUpdaterService : IDnsUpdaterService
{
  private readonly ILoggerAdapter<DnsUpdaterService> _logger;
  private readonly IDateTimeAbstraction _dateTime;
  private readonly IBasicHttpService _httpService;
  private readonly DnsUpdaterConfig _config;

  public DnsUpdaterService(IServiceProvider serviceProvider)
  {
    // TODO: [DnsUpdaterService] (TESTS) Add tests
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<DnsUpdaterService>>();
    _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();
    _httpService = serviceProvider.GetRequiredService<IBasicHttpService>();
    _config = serviceProvider.GetRequiredService<DnsUpdaterConfig>();
  }


  // Interface methods
  public async Task UpdateEntryAsync(DnsUpdaterEntry entry, CancellationToken stoppingToken)
  {
    // TODO: [TESTS] (DnsUpdaterService.UpdateEntryAsync) Add tests
    // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
    switch (entry.Type)
    {
      case DnsType.FreeDns:
        await UpdateFreeDnsEntry(entry, stoppingToken);
        break;

      default:
        HandleMissingUpdater(entry);
        break;
    }

    // Set the next update time
    entry.NextUpdate = _dateTime.Now.AddSeconds(entry.UpdateIntervalSec);
  }


  // Internal methods
  private void HandleMissingUpdater(DnsUpdaterEntry entry)
  {
    // TODO: [TESTS] (DnsUpdaterService.HandleMissingUpdater) Add tests
    _logger.LogError("No updater found for DnsEntry type '{type}'",
      entry.Type.ToString("G")
    );
  }

  private async Task UpdateFreeDnsEntry(DnsUpdaterEntry entry, CancellationToken stoppingToken)
  {
    // TODO: [TESTS] (DnsUpdaterService.UpdateFreeDnsEntry) Add tests
    var updateUrl = entry.GetConfig(ConfigKeys.Url);
    var timeoutMs = entry.GetIntConfig(ConfigKeys.TimeoutMs, _config.DefaultHttpTimeoutMs);
    var request = new HttpRequestMessage(HttpMethod.Get, updateUrl);
    var response = await _httpService.SendAsync(request, timeoutMs, stoppingToken);
    var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);

    _logger.LogInformation("Update response for {entryName}: ({code}) {body}",
      entry.Name,
      (int)response.StatusCode,
      responseBody);
  }
}