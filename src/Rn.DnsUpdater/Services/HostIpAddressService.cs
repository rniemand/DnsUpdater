using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Extensions;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Services;

namespace Rn.DnsUpdater.Services
{
  public interface IHostIpAddressService
  {
    Task<bool> HostAddressChanged(CancellationToken stoppingToken);
  }

  public class HostIpAddressService : IHostIpAddressService
  {
    private readonly ILoggerAdapter<HostIpAddressService> _logger;
    private readonly IBasicHttpService _httpService;
    private readonly IDateTimeAbstraction _dateTime;
    private readonly DnsUpdaterConfig _config;

    private string _lastHostAddress;
    private DateTime? _nextUpdate;

    public HostIpAddressService(
      ILoggerAdapter<HostIpAddressService> logger,
      IBasicHttpService httpService,
      IDateTimeAbstraction dateTime,
      DnsUpdaterConfig config)
    {
      _logger = logger;
      _httpService = httpService;
      _config = config;
      _dateTime = dateTime;

      _lastHostAddress = string.Empty;
      _nextUpdate = null;
    }


    // Interface methods
    public async Task<bool> HostAddressChanged(CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (HostIpAddressService.HostAddressChanged) Add tests
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

      try
      {
        const string requestUri = "https://api64.ipify.org/";
        var timeoutMs = _config.DefaultHttpTimeoutMs;

        _logger.Info("Refreshing hosts IP Address ({url}) timeout = {timeout} ms",
          requestUri,
          timeoutMs
        );

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
        var response = await _httpService.SendAsync(request, stoppingToken, timeoutMs);
        var hostIpAddress = await response.Content.ReadAsStringAsync(stoppingToken);

        if (!string.IsNullOrWhiteSpace(hostIpAddress))
        {
          _nextUpdate = _dateTime.Now.AddMinutes(_config.UpdateHostIpIntervalMin);
          return hostIpAddress;
        }

        _logger.Warning("Got empty response, returning old IP Address to be safe");
        return _lastHostAddress;
      }
      catch (Exception ex)
      {
        _logger.LogUnexpectedException(ex);
        return _lastHostAddress;
      }
    }
  }
}
