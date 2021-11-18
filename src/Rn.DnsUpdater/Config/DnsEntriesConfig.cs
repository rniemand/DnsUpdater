using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Rn.DnsUpdater.Config
{
  public class DnsEntriesConfig
  {
    [JsonProperty("entries"), JsonPropertyName("entries")]
    public DnsUpdaterEntry[] Entries { get; set; }

    public DnsEntriesConfig()
    {
      Entries = Array.Empty<DnsUpdaterEntry>();
    }
  }
}
