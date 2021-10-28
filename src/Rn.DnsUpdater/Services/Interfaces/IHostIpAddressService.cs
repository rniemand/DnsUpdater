using System.Threading;
using System.Threading.Tasks;

namespace Rn.DnsUpdater.Services
{
  public interface IHostIpAddressService
  {
    Task<bool> HostAddressChangedAsync(CancellationToken stoppingToken);
  }
}