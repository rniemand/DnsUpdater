using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Rn.DnsUpdater.Enums;

namespace Rn.DnsUpdater.Config
{
  public class DnsUpdaterEntry
  {
    [JsonProperty("enabled"), JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonProperty("nextUpdate"), JsonPropertyName("nextUpdate")]
    public DateTime? NextUpdate { get; set; }

    [JsonProperty("type"), JsonPropertyName("type")]
    public DnsType Type { get; set; }

    [JsonProperty("UpdateIntervalSec"), JsonPropertyName("UpdateIntervalSec")]
    public int UpdateIntervalSec { get; set; }

    [JsonProperty("config"), JsonPropertyName("config")]
    public Dictionary<string, string> Config { get; set; }


    // Constructor
    public DnsUpdaterEntry()
    {
      // TODO: [TESTS] (DnsUpdaterEntry) Add tests
      Enabled = false;
      NextUpdate = null;
      Type = DnsType.Unspecified;
      UpdateIntervalSec = 60 * 60 * 12;
      Config = new Dictionary<string, string>();
    }
  }
}