using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Enums;
using Rn.DnsUpdater.Core.Extensions;
using RnCore.Abstractions;
using RnCore.Logging;

namespace Rn.DnsUpdater.Core.Services;

public interface IDnsUpdaterService
{
  Task UpdateEntryAsync(DnsUpdaterEntry entry, CancellationToken stoppingToken);
}

public class DnsUpdaterService : IDnsUpdaterService
{
  private readonly ILoggerAdapter<DnsUpdaterService> _logger;
  private readonly IDateTimeAbstraction _dateTime;
  private readonly HttpClient _httpClient;
  private readonly DnsUpdaterConfig _config;

  public DnsUpdaterService(IServiceProvider serviceProvider)
  {
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<DnsUpdaterService>>();
    _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();
    _httpClient = new HttpClient();
    _config = serviceProvider.GetRequiredService<DnsUpdaterConfig>();
  }
  
  public async Task UpdateEntryAsync(DnsUpdaterEntry entry, CancellationToken stoppingToken)
  {
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
  
  private void HandleMissingUpdater(DnsUpdaterEntry entry)
  {
    _logger.LogError("No updater found for DnsEntry type '{type}'",
      entry.Type.ToString("G")
    );
  }

  private async Task UpdateFreeDnsEntry(DnsUpdaterEntry entry, CancellationToken stoppingToken)
  {
    var updateUrl = entry.GetConfig(ConfigKeys.Url);
    var timeoutMs = entry.GetIntConfig(ConfigKeys.TimeoutMs, _config.DefaultHttpTimeoutMs);
    var request = new HttpRequestMessage(HttpMethod.Get, updateUrl);
    var response = await _httpClient.SendAsync(request, stoppingToken);
    var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);

    _logger.LogInformation("Update response for {entryName}: ({code}) {body}",
      entry.Name,
      (int)response.StatusCode,
      responseBody);
  }
}
