using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;

namespace Rn.DnsUpdater.Services
{
  public interface IDnsUpdaterService
  {
    Task UpdateEntryAsync(DnsUpdaterEntry entry, CancellationToken stoppingToken);
  }
}