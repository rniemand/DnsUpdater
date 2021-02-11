using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rn.DnsUpdater.Services
{
  public interface IIpResolverService
  {
    Task<string> GetIpAddress(CancellationToken stoppingToken);
  }

  public class IpResolverService : IIpResolverService
  {
    public async Task<string> GetIpAddress(CancellationToken stoppingToken)
    {
      // TODO: [TESTS] (IpResolverService.GetIpAddress) Add tests
      var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ipify.org/");
      var httpClient = new HttpClient();
      var response = await httpClient.SendAsync(request, stoppingToken);
      var rawIpAddress = await response.Content.ReadAsStringAsync(stoppingToken);
      return rawIpAddress;
    }
  }
}
