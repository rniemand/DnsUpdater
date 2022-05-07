namespace Rn.DnsUpdater.Core.Services.Interfaces;

public interface IHostIpAddressService
{
  Task<bool> HostAddressChangedAsync(CancellationToken stoppingToken);
}