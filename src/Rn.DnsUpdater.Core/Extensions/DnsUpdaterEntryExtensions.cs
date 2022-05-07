using Rn.DnsUpdater.Core.Config;

namespace Rn.DnsUpdater.Core.Extensions;

public static class DnsUpdaterEntryExtensions
{
  public static string? GetConfig(this DnsUpdaterEntry config, string key, string? fallback = null) =>
    // TODO: [TESTS] (DnsUpdaterEntry.GetConfig) Add tests
    !config.Config.ContainsKey(key) ? fallback : config.Config[key];

  public static int GetIntConfig(this DnsUpdaterEntry config, string key, int fallback)
  {
    // TODO: [TESTS] (DnsUpdaterEntry.GetIntConfig) Add tests
    if (!config.Config.ContainsKey(key))
      return fallback;

    return int.TryParse(config.Config[key], out var parsed) ? parsed : fallback;
  }
}
