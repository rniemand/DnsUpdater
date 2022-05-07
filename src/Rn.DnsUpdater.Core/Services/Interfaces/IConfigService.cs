using Rn.DnsUpdater.Core.Config;

namespace Rn.DnsUpdater.Core.Services.Interfaces;

public interface IConfigService
{
  DnsUpdaterConfig CoreConfig { get; }
  DnsEntriesConfig DnsEntriesConfig { get; }

  List<DnsUpdaterEntry> GetEntriesNeedingUpdate();
  List<DnsUpdaterEntry> GetEnabledEntries();
  void SaveConfigState();
}
