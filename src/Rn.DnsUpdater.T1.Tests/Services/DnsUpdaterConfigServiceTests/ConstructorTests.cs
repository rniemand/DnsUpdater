using DocumentSink.ClientLib;
using NSubstitute;
using NUnit.Framework;
using Rn.DnsUpdater.Services;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.T1.Tests.Services.DnsUpdaterConfigServiceTests
{
  [TestFixture]
  public class ConstructorTests
  {
    [Test]
    public void DnsUpdaterConfigService_Given_Constructed_ShouldResolve_RequiredServices()
    {
      // arrange
      var serviceProvider = TestHelper.GetServiceProvider();

      // act
      var _ = new DnsUpdaterConfigService(serviceProvider);

      // assert
      serviceProvider.Received(1).GetService(typeof(ILoggerAdapter<DnsUpdaterConfigService>));
      serviceProvider.Received(1).GetService(typeof(IPathAbstraction));
      serviceProvider.Received(1).GetService(typeof(IDirectoryAbstraction));
      serviceProvider.Received(1).GetService(typeof(IFileAbstraction));
      serviceProvider.Received(1).GetService(typeof(IJsonHelper));
      serviceProvider.Received(1).GetService(typeof(IDateTimeAbstraction));
      serviceProvider.Received(1).GetService(typeof(IEnvironmentAbstraction));
    }
  }
}
