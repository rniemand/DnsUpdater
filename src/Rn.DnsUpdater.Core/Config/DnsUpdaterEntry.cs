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
}
