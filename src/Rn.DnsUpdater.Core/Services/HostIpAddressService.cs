using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.NetCore.BasicHttp;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Extensions;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Core.Services;

public interface IHostIpAddressService
{
  Task<bool> HostAddressChangedAsync(CancellationToken stoppingToken);
}

public class HostIpAddressService : IHostIpAddressService
{
  private readonly ILoggerAdapter<HostIpAddressService> _logger;
  private readonly IBasicHttpService _httpService;
  private readonly IDateTimeAbstraction _dateTime;
  private readonly DnsUpdaterConfig _config;

  private string _lastHostAddress;
  private DateTime? _nextUpdate;

  public HostIpAddressService(IServiceProvider serviceProvider)
  {
    // TODO: [HostIpAddressService] (TESTS) Add tests
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<HostIpAddressService>>();
    _httpService = serviceProvider.GetRequiredService<IBasicHttpService>();
    _config = serviceProvider.GetRequiredService<DnsUpdaterConfig>();
    _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();

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

    _logger.LogInformation("Host IP Address changed from '{old}' to '{new}'",
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
    if (!_nextUpdate.HasValue)
      return true;

    return !(_nextUpdate > _dateTime.Now);
  }

  private async Task<string> GetHostAddress(CancellationToken stoppingToken)
  {
    // TODO: [TESTS] (HostIpAddressService.GetHostAddress) Add tests
    if (!HostAddressNeedsUpdating())
      return _lastHostAddress;

    //const string url = "https://api64.ipify.org/";
    const string url = "https://api.ipify.org/";
    var timeout = _config.DefaultHttpTimeoutMs;

    _logger.LogInformation("Refreshing hosts IP Address ({url}) timeout = {timeout} ms", url, timeout);

    var request = new HttpRequestMessage(HttpMethod.Get, url);
    var response = await _httpService.SendAsync(request, timeout, stoppingToken);
    var hostIpAddress = (await response.Content.ReadAsStringAsync(stoppingToken)).LowerTrim();
    
    if (!string.IsNullOrWhiteSpace(hostIpAddress))
    {
      _nextUpdate = _dateTime.Now.AddMinutes(_config.UpdateHostIpIntervalMin);
      return hostIpAddress;
    }

    _logger.LogWarning("Got empty response, returning old IP Address to be safe");
    return _lastHostAddress;
  }
}
