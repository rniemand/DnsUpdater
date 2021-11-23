using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;
using Rn.DnsUpdater.Metrics;
using Rn.NetCore.BasicHttp;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Extensions;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics;
using Rn.NetCore.Metrics.Builders;

namespace Rn.DnsUpdater.Services
{
  public class HostIpAddressService : IHostIpAddressService
  {
    private readonly ILoggerAdapter<HostIpAddressService> _logger;
    private readonly IBasicHttpService _httpService;
    private readonly IDateTimeAbstraction _dateTime;
    private readonly IMetricService _metrics;
    private readonly DnsUpdaterConfig _config;

    private string _lastHostAddress;
    private DateTime? _nextUpdate;

    public HostIpAddressService(
      ILoggerAdapter<HostIpAddressService> logger, 
      IBasicHttpService httpService,
      IDateTimeAbstraction dateTime,
      IMetricService metrics,
      DnsUpdaterConfig config)
    {
      _logger = logger;
      _httpService = httpService;
      _config = config;
      _metrics = metrics;
      _dateTime = dateTime;

      _lastHostAddress = string.Empty;
      _nextUpdate = null;
    }


    // Interface methods
    public async Task<bool> HostAddressChangedAsync(CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (HostIpAddressService.HostAddressChangedAsync) Add tests
      var hostAddress = await GetHostAddress(stoppingToken);

      if (_lastHostAddress.IgnoreCaseEquals(hostAddress))
        return false;

      _logger.Info("Host IP Address changed from '{old}' to '{new}'",
        _lastHostAddress.FallbackTo("(none)"),
        hostAddress
      );

      _lastHostAddress = hostAddress.LowerTrim();
      return true;
    }


    // Internal methods
    private bool HostAddressNeedsUpdating()
    {
      // TODO: [TESTS] (HostIpAddressService.HostAddressNeedsUpdating) Add tests
      if (_nextUpdate.HasValue == false)
        return true;

      return !(_nextUpdate > _dateTime.Now);
    }

    private async Task<string> GetHostAddress(CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (HostIpAddressService.GetHostAddress) Add tests
      if (!HostAddressNeedsUpdating())
        return _lastHostAddress;

      var builder = new ServiceMetricBuilder(nameof(HostIpAddressService), nameof(GetHostAddress))
        .WithCategory(MetricCategory.HostIpAddress, MetricSubCategory.Tick)
        .WithCustomTag1(_lastHostAddress.FallbackTo(MetricPlaceholder.Unknown)) // Old Address
        .WithCustomTag2(MetricPlaceholder.Unknown) // New Address
        .WithCustomTag3(MetricPlaceholder.Unknown) // Status code
        .WithCustomTag4(false) // Changed
        .WithCustomInt1(0);

      try
      {
        using (builder.WithTiming())
        {
          //const string url = "https://api64.ipify.org/";
          const string url = "https://api.ipify.org/";
          var timeout = _config.DefaultHttpTimeoutMs;

          _logger.Info("Refreshing hosts IP Address ({url}) timeout = {timeout} ms", url, timeout);
          builder.WithCustomInt1(timeout);

          var request = new HttpRequestMessage(HttpMethod.Get, url);
          var response = await _httpService.SendAsync(request, timeout, stoppingToken);
          var hostIpAddress = (await response.Content.ReadAsStringAsync(stoppingToken)).LowerTrim();

          builder
            .WithCustomTag2(hostIpAddress)
            .WithCustomTag3(response.StatusCode.ToString("G"), true)
            .WithCustomTag4(!_lastHostAddress.IgnoreCaseEquals(hostIpAddress));

          if (!string.IsNullOrWhiteSpace(hostIpAddress))
          {
            _nextUpdate = _dateTime.Now.AddMinutes(_config.UpdateHostIpIntervalMin);
            return hostIpAddress;
          }

          _logger.Warning("Got empty response, returning old IP Address to be safe");
          return _lastHostAddress;
        }
      }
      catch (Exception ex)
      {
        _logger.LogUnexpectedException(ex);
        return _lastHostAddress;
      }
      finally
      {
        await _metrics.SubmitBuilderAsync(builder);
      }
    }
  }
}
