using Rn.DnsUpdater.Config;

namespace Rn.DnsUpdater.T1.Tests.TestSupport.Builders
{
  public class DnsUpdaterConfigBuilder
  {
    private readonly DnsUpdaterConfig _config;

    public DnsUpdaterConfigBuilder()
    {
      _config = new DnsUpdaterConfig();
    }

    public DnsUpdaterConfigBuilder WithDefaults()
    {
      _config.ConfigFile = "./dns.config.json";
      _config.TickInterval = 5000;
      _config.UpdateHostIpIntervalMin = 10;
      _config.DefaultHttpTimeoutMs = 5000;

      return this;
    }

    public DnsUpdaterConfig Build() => _config;

    public DnsUpdaterConfig BuildWithDefaults()
      => WithDefaults().Build();
  }
}
