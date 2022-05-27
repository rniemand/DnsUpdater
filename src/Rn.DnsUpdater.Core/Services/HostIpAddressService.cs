using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Exceptions;
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
  private readonly string _providerUrl;
  private readonly int _httpTimeoutMs;

  private string _lastHostAddress;
  private DateTime? _nextUpdate;

  public HostIpAddressService(IServiceProvider serviceProvider)
  {
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<HostIpAddressService>>();
    _httpService = serviceProvider.GetRequiredService<IBasicHttpService>();
    _config = serviceProvider.GetRequiredService<DnsUpdaterConfig>();
    _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();

    _providerUrl = GetProviderUrl("ipify");
    _httpTimeoutMs = _config.DefaultHttpTimeoutMs;

    _lastHostAddress = string.Empty;
    _nextUpdate = null;
  }
  
  public async Task<bool> HostAddressChangedAsync(CancellationToken stoppingToken)
  {
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
  
  private bool HostAddressNeedsUpdating()
  {
    if (!_nextUpdate.HasValue)
      return true;

    return !(_nextUpdate > _dateTime.Now);
  }

  private string GetProviderUrl(string provider)
  {
    if (_config.ProviderUrls.Count == 0)
      throw new MissingProviderUrlException(provider);

    if(!_config.ProviderUrls.Any(x => x.Key.IgnoreCaseEquals(provider)))
      throw new MissingProviderUrlException(provider);

    return _config.ProviderUrls
      .First(x => x.Key.IgnoreCaseEquals(provider))
      .Value;
  }

  private async Task<string> GetHostAddress(CancellationToken stoppingToken)
  {
    if (!HostAddressNeedsUpdating())
      return _lastHostAddress;

    _logger.LogInformation("Refreshing hosts IP Address ({url}) timeout = {timeout} ms",
      _providerUrl,
      _httpTimeoutMs);

    var request = new HttpRequestMessage(HttpMethod.Get, _providerUrl);
    var response = await _httpService.SendAsync(request, _httpTimeoutMs, stoppingToken);
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
