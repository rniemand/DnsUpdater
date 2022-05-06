using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Enums;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics;
using Rn.NetCore.Metrics.Builders;

namespace Rn.DnsUpdater.Services;

public class HeartbeatService : IHeartbeatService
{
  private readonly ILoggerAdapter<HeartbeatService> _logger;
  private readonly IMetricService _metrics;
  private readonly IDateTimeAbstraction _dateTime;

  private DateTime? _nextTick;
  private readonly DateTime _startTime;

  public HeartbeatService(IServiceProvider serviceProvider)
  {
    // TODO: [HeartbeatService.HeartbeatService] (TESTS) Add tests
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<HeartbeatService>>();
    _metrics = serviceProvider.GetRequiredService<IMetricService>();
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

    try
    {
      var runningTime = (_dateTime.Now - _startTime);
      _logger.LogDebug("Heartbeat - running for {time}", runningTime.ToString("g"));

      await _metrics.SubmitBuilderAsync(new ServiceMetricBuilder(nameof(HeartbeatService), nameof(TickAsync))
        .WithCategory(MetricCategory.Heartbeat, MetricSubCategory.Tick)
        .WithCustomLong1((long)runningTime.TotalSeconds)
      );
    }
    catch (Exception ex)
    {
      _logger.LogUnexpectedException(ex);
    }

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
