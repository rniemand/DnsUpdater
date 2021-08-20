using System;
using System.Threading.Tasks;
using DocumentSink.ClientLib;
using Rn.DnsUpdater.Enums;
using Rn.DnsUpdater.Extensions;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Metrics.Builders;
using Rn.NetCore.Common.Metrics.Interfaces;

namespace Rn.DnsUpdater.Services
{
  public interface IHeartbeatService
  {
    Task Tick();
  }

  public class HeartbeatService : IHeartbeatService
  {
    private readonly IDocumentSinkClient _logger;
    private readonly IMetricService _metrics;
    private readonly IDateTimeAbstraction _dateTime;

    private DateTime? _nextTick;
    private readonly DateTime _startTime;

    public HeartbeatService(
      IDocumentSinkClient documentSink, 
      IMetricService metrics, 
      IDateTimeAbstraction dateTime)
    {
      _logger = documentSink;
      _metrics = metrics;
      _dateTime = dateTime;
      
      _nextTick = null;
      _startTime = dateTime.Now;
    }


    // Interface methods
    public async Task Tick()
    {
      // TODO: [TESTS] (HeartbeatService.Tick) Add tests
      if(!CanRunHeartbeat())
        return;

      try
      {
        var runningTime = (_dateTime.Now - _startTime);
        _logger.Debug("Heartbeat - running for {time}", runningTime.ToString("g"));

        await _metrics.SubmitBuilderAsync(new ServiceMetricBuilder(nameof(HeartbeatService), nameof(Tick))
          .WithCategory(MetricCategory.Heartbeat, MetricSubCategory.Tick)
          .WithCustomLong1((long) runningTime.TotalSeconds)
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
      if (_nextTick.HasValue == false)
        return true;

      return !(_nextTick > _dateTime.Now);
    }
  }
}
