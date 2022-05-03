using System.Collections.Generic;
using NUnit.Framework;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;

namespace Rn.DnsUpdater.T1.Tests.Config;

[TestFixture]
public class DnsUpdaterEntryTests
{
  [Test]
  public void DnsUpdaterEntry_Given_Constructed_ShouldDefault_Enabled()
  {
    // act
    var entry = new DnsUpdaterEntry();

    // assert
    Assert.IsFalse(entry.Enabled);
  }

  [Test]
  public void DnsUpdaterEntry_Given_Constructed_ShouldDefault_NextUpdate()
  {
    // act
    var entry = new DnsUpdaterEntry();

    // assert
    Assert.IsNull(entry.NextUpdate);
  }

  [Test]
  public void DnsUpdaterEntry_Given_Constructed_ShouldDefault_Name()
  {
    // act
    var entry = new DnsUpdaterEntry();

    // assert
    Assert.AreEqual(string.Empty, entry.Name);
  }

  [Test]
  public void DnsUpdaterEntry_Given_Constructed_ShouldDefault_Type()
  {
    // act
    var entry = new DnsUpdaterEntry();

    // assert
    Assert.AreEqual(DnsType.Unspecified, entry.Type);
  }

  [Test]
  public void DnsUpdaterEntry_Given_Constructed_ShouldDefault_UpdateIntervalSec()
  {
    // act
    var entry = new DnsUpdaterEntry();

    // assert
    Assert.AreEqual(43200, entry.UpdateIntervalSec);
  }

  [Test]
  public void DnsUpdaterEntry_Given_Constructed_ShouldDefault_Config()
  {
    // act
    var entry = new DnsUpdaterEntry();

    // assert
    Assert.IsNotNull(entry.Config);
    Assert.IsInstanceOf<Dictionary<string, string>>(entry.Config);
    Assert.AreEqual(0, entry.Config.Count);
  }
}