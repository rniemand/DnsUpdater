using Newtonsoft.Json;
using Rn.DnsUpdater.Core.Enums;

namespace Rn.DnsUpdater.Core.Config;

// DOCS: docs\configuration\DnsUpdaterEntry.md
public class DnsUpdaterEntry
{
  [JsonProperty("enabled")]
  public bool Enabled { get; set; }

  [JsonProperty("name")]
  public string Name { get; set; } = string.Empty;

  [JsonProperty("nextUpdate")]
  public DateTime? NextUpdate { get; set; }

  [JsonProperty("type")]
  public DnsType Type { get; set; } = DnsType.Unspecified;

  [JsonProperty("updateIntervalSec")]
  public int UpdateIntervalSec { get; set; } = 43200;

  [JsonProperty("config")]
  public Dictionary<string, string> Config { get; set; } = new();

  
  // Helper methods
  public string? GetConfig(string key, string? fallback = null) =>
    // TODO: [TESTS] (DnsUpdaterEntry.GetConfig) Add tests
    !Config.ContainsKey(key) ? fallback : Config[key];

  public int GetIntConfig(string key, int fallback)
  {
    // TODO: [TESTS] (DnsUpdaterEntry.GetIntConfig) Add tests
    if (!Config.ContainsKey(key))
      return fallback;

    return int.TryParse(Config[key], out var parsed) ? parsed : fallback;
  }
}
