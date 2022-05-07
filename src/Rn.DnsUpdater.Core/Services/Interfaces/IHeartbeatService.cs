namespace Rn.DnsUpdater.Core.Services.Interfaces;

public interface IHeartbeatService
{
  Task TickAsync();
}