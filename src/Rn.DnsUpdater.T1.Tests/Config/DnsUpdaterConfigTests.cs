using NUnit.Framework;
using Rn.DnsUpdater.Config;

namespace Rn.DnsUpdater.T1.Tests.Config
{
  [TestFixture]
  public class DnsUpdaterConfigTests
  {
    [Test]
    public void DnsUpdaterConfig_Given_Constructed_ShouldDefault_ConfigFile()
    {
      Assert.AreEqual("./dns.config.json",
        new DnsUpdaterConfig().ConfigFile
      );
    }

    [Test]
    public void DnsUpdaterConfig_Given_Constructed_ShouldDefault_TickInterval()
    {
      Assert.AreEqual(5000,
        new DnsUpdaterConfig().TickInterval
      );
    }

    [Test]
    public void DnsUpdaterConfig_Given_Constructed_ShouldDefault_UpdateHostIpIntervalMin()
    {
      Assert.AreEqual(10,
        new DnsUpdaterConfig().UpdateHostIpIntervalMin
      );
    }

    [Test]
    public void DnsUpdaterConfig_Given_Constructed_ShouldDefault_DefaultHttpTimeoutMs()
    {
      Assert.AreEqual(5000,
        new DnsUpdaterConfig().DefaultHttpTimeoutMs
      );
    }
  }
}
