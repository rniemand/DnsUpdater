using Newtonsoft.Json;

namespace Rn.DnsUpdater.Core.Config;

// DOCS: docs\configuration\DnsEntriesConfig.md
public class DnsEntriesConfig
{
  [JsonProperty("entries")]
  public DnsUpdaterEntry[] Entries { get; set; } = Array.Empty<DnsUpdaterEntry>();
}
