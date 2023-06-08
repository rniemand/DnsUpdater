using RnCore.Metrics.Builders;

namespace Rn.DnsUpdater.Core;

public sealed class ServiceMetricBuilder : BaseMetricBuilder<ServiceMetricBuilder>
{
  public ServiceMetricBuilder()
    : base("service_call")
  {
    SetSuccess(true);
    AddAction(m => m.SetField("call_count", 1));
  }

  public ServiceMetricBuilder(string serviceName)
    : this()
  {
    WithName(serviceName);
  }

  public ServiceMetricBuilder WithException(Exception ex)
  {
    SetException(ex);
    return this;
  }

  public ServiceMetricBuilder WithName(string name)
  {
    AddAction(m => m.SetTag("name", name, skipToLower: true));
    return this;
  }

  public ServiceMetricBuilder WithDnsEntryCount(int count)
  {
    AddAction(m => m.SetField("entry_count", count));
    return this;
  }
}
