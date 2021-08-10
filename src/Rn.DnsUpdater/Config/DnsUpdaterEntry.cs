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

    [JsonProperty("name"), JsonPropertyName("name")]
    public string Name { get; set; }

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
      Enabled = false;
      NextUpdate = null;
      Name = string.Empty;
      Type = DnsType.Unspecified;
      UpdateIntervalSec = 60 * 60 * 12;
      Config = new Dictionary<string, string>();
    }


    // Helper methods
    public string GetConfig(string key, string fallback = null)
    {
      // TODO: [TESTS] (DnsUpdaterEntry.GetConfig) Add tests
      return !Config.ContainsKey(key) ? fallback : Config[key];
    }

    public int GetIntConfig(string key, int fallback)
    {
      // TODO: [TESTS] (DnsUpdaterEntry.GetIntConfig) Add tests
      if (!Config.ContainsKey(key))
        return fallback;

      return int.TryParse(Config[key], out var parsed) ? parsed : fallback;
    }
  }
}