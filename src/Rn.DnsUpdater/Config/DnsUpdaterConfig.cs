using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Rn.DnsUpdater.Config
{
  public class DnsUpdaterConfig
  {
    [JsonProperty("ConfigFile"), JsonPropertyName("ConfigFile")]
    public string ConfigFile { get; set; }

    [JsonProperty("TickInterval"), JsonPropertyName("TickInterval")]
    public int TickInterval { get; set; }

    [JsonProperty("UpdateHostIpIntervalMin"), JsonPropertyName("UpdateHostIpIntervalMin")]
    public int UpdateHostIpIntervalMin { get; set; }

    public DnsUpdaterConfig()
    {
      // TODO: [TESTS] (DnsUpdaterConfig.DnsUpdaterConfig) Add tests
      ConfigFile = "./dns.config.json";
      TickInterval = 5000;
      UpdateHostIpIntervalMin = 10;
    }
  }
}
