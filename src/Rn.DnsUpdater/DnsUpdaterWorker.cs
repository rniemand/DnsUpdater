using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Core;

namespace Rn.DnsUpdater;

public class DnsUpdaterWorker : BackgroundService
{
  private readonly IDnsUpdateRunner _updateRunner;

  public DnsUpdaterWorker(IDnsUpdateRunner updateRunner)
  {
    _updateRunner = updateRunner;
  }

  // Required methods
  protected override async Task ExecuteAsync(CancellationToken stoppingToken) =>
    await _updateRunner.ExecuteAsync(stoppingToken);
}
