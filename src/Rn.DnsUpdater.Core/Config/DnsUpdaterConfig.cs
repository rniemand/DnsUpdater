using Microsoft.Extensions.Configuration;

namespace Rn.DnsUpdater.Core.Config;

// DOCS: docs\configuration\appsettings.md
public class DnsUpdaterConfig
{
  [ConfigurationKeyName("configFile")]
  public string ConfigFile { get; set; } = "./dns.config.json";

  [ConfigurationKeyName("tickInterval")]
  public int TickInterval { get; set; } = 5000;

  [ConfigurationKeyName("updateHostIpIntervalMin")]
  public int UpdateHostIpIntervalMin { get; set; } = 10;

  [ConfigurationKeyName("defaultHttpTimeoutMs")]
  public int DefaultHttpTimeoutMs { get; set; } = 5000;

  [ConfigurationKeyName("providerUrls")]
  public Dictionary<string, string> ProviderUrls { get; set; } = new();
}
