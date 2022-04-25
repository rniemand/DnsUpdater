using NUnit.Framework;
using Rn.DnsUpdater.Config;

namespace Rn.DnsUpdater.T1.Tests.Config;

[TestFixture]
public class DnsEntriesConfigTests
{
  [Test]
  public void DnsEntriesConfig_Given_Constructed_ShouldDefault_Entries()
  {
    // act
    var config = new DnsEntriesConfig();

    // assert
    Assert.NotNull(config);
    Assert.IsInstanceOf<DnsUpdaterEntry[]>(config.Entries);
    Assert.AreEqual(0, config.Entries.Length);
  }
}