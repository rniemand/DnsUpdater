using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Services.Interfaces;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Core.Services;

public class HeartbeatService : IHeartbeatService
{
  private readonly ILoggerAdapter<HeartbeatService> _logger;
  private readonly IDateTimeAbstraction _dateTime;

  private DateTime? _nextTick;
  private readonly DateTime _startTime;

  public HeartbeatService(IServiceProvider serviceProvider)
  {
    // TODO: [HeartbeatService.HeartbeatService] (TESTS) Add tests
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<HeartbeatService>>();
    _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();

    _nextTick = null;
    _startTime = _dateTime.Now;
  }


  // Interface methods
  public async Task TickAsync()
  {
    // TODO: [TESTS] (HeartbeatService.Tick) Add tests
    if (!CanRunHeartbeat())
      return;

    var runningTime = (_dateTime.Now - _startTime);
    _logger.LogDebug("Heartbeat - running for {time}", runningTime.ToString("g"));
    _nextTick = _dateTime.Now.AddSeconds(60);
  }


  // Internal methods
  private bool CanRunHeartbeat()
  {
    // TODO: [TESTS] (HeartbeatService.CanRunHeartbeat) Add tests
    if (!_nextTick.HasValue)
      return true;

    return !(_nextTick > _dateTime.Now);
  }
}
