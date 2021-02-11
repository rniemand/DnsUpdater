namespace Rn.DnsUpdater.Config
{
  public class DnsUpdaterConfig
  {
    public string ConfigFile { get; set; }

    public DnsUpdaterConfig()
    {
      // TODO: [TESTS] (DnsUpdaterConfig.DnsUpdaterConfig) Add tests
      ConfigFile = "./dns.config.json";
    }
  }
}
