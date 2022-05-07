using Rn.DnsUpdater.Core.Config;

namespace Rn.DnsUpdater.Core.Services.Interfaces;

public interface IDnsUpdaterService
{
  Task UpdateEntryAsync(DnsUpdaterEntry entry, CancellationToken stoppingToken);
}