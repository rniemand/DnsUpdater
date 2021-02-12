using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Services
{
  public interface IDnsUpdaterService
  {
    Task UpdateDnsEntry(DnsUpdaterEntry entry);
  }

  public class DnsUpdaterService : IDnsUpdaterService
  {
    private readonly ILoggerAdapter<DnsUpdaterService> _logger;

    public DnsUpdaterService(ILoggerAdapter<DnsUpdaterService> logger)
    {
      _logger = logger;
    }


    // Interface
    public async Task UpdateDnsEntry(DnsUpdaterEntry entry)
    {
      // TODO: [TESTS] (DnsUpdaterService.UpdateDnsEntry) Add tests
      await Task.CompletedTask;
    }
  }
}
