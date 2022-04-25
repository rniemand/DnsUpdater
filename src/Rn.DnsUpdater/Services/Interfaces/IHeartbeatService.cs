using System.Threading.Tasks;

namespace Rn.DnsUpdater.Services;

public interface IHeartbeatService
{
  Task TickAsync();
}