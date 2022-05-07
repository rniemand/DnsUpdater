using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Services;
using Rn.DnsUpdater.Core.Services.Interfaces;
using Rn.NetCore.BasicHttp;
using Rn.NetCore.BasicHttp.Factories;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics;
using Rn.NetCore.Metrics.Outputs;
using Rn.NetCore.Metrics.Rabbit;
using Rn.NetCore.Metrics.Rabbit.Interfaces;

namespace Rn.DnsUpdater.Core.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRnDnsUpdater(this IServiceCollection services, IConfiguration configuration)
  {
    // TODO: [ServiceCollectionExtensions.AddRnDnsUpdater] (TESTS) Add tests
    return services
      // Abstractions
      .AddSingleton<IFileAbstraction, FileAbstraction>()
      .AddSingleton<IDirectoryAbstraction, DirectoryAbstraction>()
      .AddSingleton<IEnvironmentAbstraction, EnvironmentAbstraction>()
      .AddSingleton<IPathAbstraction, PathAbstraction>()
      .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()

      // Config
      .AddSingleton(GenerateConfig(configuration))
      .AddSingleton(configuration)

      // Factories
      .AddSingleton<IHttpClientFactory, HttpClientFactory>()

      // Helpers
      .AddSingleton<IJsonHelper, JsonHelper>()

      // Logging
      .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))

      // Metrics
      .AddSingleton<IMetricService, MetricService>()
      .AddSingleton<IMetricServiceUtils, MetricServiceUtils>()
      .AddSingleton<IMetricOutput, RabbitMetricOutput>()
      .AddSingleton<IRabbitConnection, RabbitConnection>()
      .AddSingleton<IRabbitFactory, RabbitFactory>()

      // Misc
      .AddSingleton<IDnsUpdateRunner, DnsUpdateRunner>()

      // Services
      .AddSingleton<IHostIpAddressService, HostIpAddressService>()
      .AddSingleton<IBasicHttpService, BasicHttpService>()
      .AddSingleton<IDnsUpdaterService, DnsUpdaterService>()
      .AddSingleton<IConfigService, ConfigService>()
      .AddSingleton<IHeartbeatService, HeartbeatService>();
  }

  private static DnsUpdaterConfig GenerateConfig(IConfiguration configuration)
  {
    // TODO: [ServiceCollectionExtensions.GenerateConfig] (TESTS) Add tests
    var boundConfig = new DnsUpdaterConfig();
    var section = configuration.GetSection("DnsUpdater");

    if (section.Exists())
      section.Bind(boundConfig);

    return boundConfig;
  }
}
