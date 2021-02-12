using System.Net.Http;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Services;

namespace Rn.DnsUpdater.Services
{
  public interface IHostIpAddressService
  {
    Task<bool> HostAddressChanged();
  }

  public class HostIpAddressService : IHostIpAddressService
  {
    private readonly ILoggerAdapter<HostIpAddressService> _logger;
    private readonly IBasicHttpService _httpService;
    private readonly DnsUpdaterConfig _config;
    private string _lastHostAddress;

    public HostIpAddressService(
      ILoggerAdapter<HostIpAddressService> logger,
      IBasicHttpService httpService,
      DnsUpdaterConfig config)
    {
      _logger = logger;
      _httpService = httpService;
      _config = config;

      _lastHostAddress = string.Empty;
    }


    // Interface methods
    public async Task<bool> HostAddressChanged()
    {
      // TODO: [TESTS] (HostIpAddressService.HostAddressChanged) Add tests
      await RefreshHostAddress();

      return false;
    }


    // Internal methods
    private async Task RefreshHostAddress()
    {
      // TODO: [TESTS] (HostIpAddressService.RefreshHostAddress) Add tests

      var updateInterval = _config.UpdateHostIpIntervalMin;

      var request = new HttpRequestMessage(HttpMethod.Get, "https://api64.ipify.org/");
      var response = await _httpService.SendAsync(request);
      var responseBody = await response.Content.ReadAsStringAsync();


    }
  }
}
