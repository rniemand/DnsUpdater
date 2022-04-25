using System.Collections.Generic;
using Rn.DnsUpdater.Config;

namespace Rn.DnsUpdater.Services;

public interface IConfigService
{
  DnsUpdaterConfig CoreConfig { get; }
  DnsEntriesConfig DnsEntriesConfig { get; }

  List<DnsUpdaterEntry> GetEntriesNeedingUpdate();
  List<DnsUpdaterEntry> GetEnabledEntries();
  void SaveConfigState();
}
