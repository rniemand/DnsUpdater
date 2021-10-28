using System;
using NSubstitute;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Services;
using Rn.DnsUpdater.T1.Tests.TestSupport.Builders;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.TestingCore.Builders;

namespace Rn.DnsUpdater.T1.Tests.Services.DnsUpdaterConfigServiceTests
{
  public static class TestHelper
  {
    public static IServiceProvider GetServiceProvider(
      ILoggerAdapter<ConfigService> logger = null,
      IPathAbstraction path = null,
      IDirectoryAbstraction directory = null,
      IFileAbstraction file = null,
      IJsonHelper jsonHelper = null,
      IDateTimeAbstraction dateTime = null,
      IEnvironmentAbstraction environment = null,
      DnsUpdaterConfig config = null)
    {
      config ??= new DnsUpdaterConfigBuilder().BuildWithDefaults();

      return new ServiceProviderBuilder()
        .WithLogger(logger)
        .WithService(path ?? Substitute.For<IPathAbstraction>())
        .WithService(directory ?? Substitute.For<IDirectoryAbstraction>())
        .WithService(file ?? Substitute.For<IFileAbstraction>())
        .WithService(jsonHelper ?? Substitute.For<IJsonHelper>())
        .WithService(dateTime ?? Substitute.For<IDateTimeAbstraction>())
        .WithService(environment ?? Substitute.For<IEnvironmentAbstraction>())
        .WithService(config)
        .Build();
    }
  }
}
