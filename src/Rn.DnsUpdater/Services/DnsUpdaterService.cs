using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;
using Rn.DnsUpdater.Metrics;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Metrics.Interfaces;
using Rn.NetCore.Common.Services;

namespace Rn.DnsUpdater.Services
{
  public interface IDnsUpdaterService
  {
    Task UpdateDnsEntry(DnsUpdaterEntry entry, CancellationToken stoppingToken);
  }

  public class DnsUpdaterService : IDnsUpdaterService
  {
    private readonly ILoggerAdapter<DnsUpdaterService> _logger;
    private readonly IDateTimeAbstraction _dateTime;
    private readonly IBasicHttpService _httpService;
    private readonly IMetricService _metrics;
    private readonly DnsUpdaterConfig _config;

    public DnsUpdaterService(
      ILoggerAdapter<DnsUpdaterService> logger,
      IDateTimeAbstraction dateTime,
      IBasicHttpService httpService,
      IMetricService metrics,
      DnsUpdaterConfig config)
    {
      _logger = logger;
      _dateTime = dateTime;
      _httpService = httpService;
      _config = config;
      _metrics = metrics;
    }


    // Interface methods
    public async Task UpdateDnsEntry(DnsUpdaterEntry entry, CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (DnsUpdaterService.UpdateDnsEntry) Add tests
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
      _logger.Error("No updater found for DnsEntry type '{type}'",
        entry.Type.ToString("G")
      );
    }

    private async Task UpdateFreeDnsEntry(DnsUpdaterEntry entry, CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (DnsUpdaterService.UpdateFreeDnsEntry) Add tests
      var builder = new UpdateDnsEntryMetricBuilder()
        .WithCategory(MetricCategory.DnsUpdater, MetricSubCategory.UpdateEntry)
        .ForDnsEntry(entry);

      try
      {
        using (builder.WithTiming())
        {
          var updateUrl = entry.GetConfig(ConfigKeys.Url);
          var timeoutMs = entry.GetIntConfig(ConfigKeys.TimeoutMs, _config.DefaultHttpTimeoutMs);
          var request = new HttpRequestMessage(HttpMethod.Get, updateUrl);
          var response = await _httpService.SendAsync(request, timeoutMs, stoppingToken);
          var responseBody = await response.Content.ReadAsStringAsync(stoppingToken);
          builder.WithResponse(response);

          _logger.Info("Update response for {entryName}: ({code}) {body}",
            entry.Name,
            (int) response.StatusCode,
            responseBody
          );
        }
      }
      catch (Exception ex)
      {
        _logger.LogUnexpectedException(ex);
        builder.WithException(ex);
      }
      finally
      {
        await _metrics.SubmitBuilderAsync(builder);
      }
    }
  }
}
