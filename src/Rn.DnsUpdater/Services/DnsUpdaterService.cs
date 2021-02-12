using System;
using System.Net.Http;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Services;

namespace Rn.DnsUpdater.Services
{
  public interface IDnsUpdaterService
  {
    Task UpdateDnsEntry(DnsUpdaterEntry entry);
  }

  public class DnsUpdaterService : IDnsUpdaterService
  {
    private readonly ILoggerAdapter<DnsUpdaterService> _logger;
    private readonly IDateTimeAbstraction _dateTime;
    private readonly IBasicHttpService _httpService;

    public DnsUpdaterService(
      ILoggerAdapter<DnsUpdaterService> logger,
      IDateTimeAbstraction dateTime,
      IBasicHttpService httpService)
    {
      _logger = logger;
      _dateTime = dateTime;
      _httpService = httpService;
    }


    // Interface
    public async Task UpdateDnsEntry(DnsUpdaterEntry entry)
    {
      // TODO: [TESTS] (DnsUpdaterService.UpdateDnsEntry) Add tests
      // Update the DnsEntry
      switch (entry.Type)
      {
        case DnsType.FreeDns:
          await UpdateFreeDnsEntry(entry);
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
      _logger.Error("No updater found for DnsEntry type '{type}'",
        entry.Type.ToString("G")
      );
    }

    private async Task UpdateFreeDnsEntry(DnsUpdaterEntry entry)
    {
      // TODO: [TESTS] (DnsUpdaterService.UpdateFreeDnsEntry) Add tests
      try
      {
        var updateUrl = entry.GetConfig(ConfigKeys.Url);
        var request = new HttpRequestMessage(HttpMethod.Get, updateUrl);
        var response = await _httpService.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        _logger.Info("Update response for {entryName}: ({code}) {body}",
          entry.Name,
          (int) response.StatusCode,
          responseBody
        );
      }
      catch (Exception ex)
      {
        _logger.LogUnexpectedException(ex);
      }
    }
  }
}
