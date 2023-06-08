using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Services;
using RnCore.Abstractions;
using RnCore.Logging;
using RnCore.Metrics.Extensions;
using RnCore.Metrics.InfluxDb;

namespace Rn.DnsUpdater.Core.Extensions;

public static class ServiceCollectionExtensions
{
  public static IServiceCollection AddRnDnsUpdater(this IServiceCollection services, IConfiguration configuration)
  {
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

      // Helpers
      .AddSingleton<IJsonHelper, JsonHelper>()

      // Logging
      .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))
      
      // Misc
      .AddSingleton<IDnsUpdateRunner, DnsUpdateRunner>()

      // Metrics
      .AddRnCoreMetrics()
      .AddInfluxDbMetricOutput()

      // Services
      .AddSingleton<IHostIpAddressService, HostIpAddressService>()
      .AddSingleton<IDnsUpdaterService, DnsUpdaterService>()
      .AddSingleton<IConfigService, ConfigService>();
  }

  private static DnsUpdaterConfig GenerateConfig(IConfiguration configuration)
  {
    var boundConfig = new DnsUpdaterConfig();
    var section = configuration.GetSection("DnsUpdater");

    if (section.Exists())
      section.Bind(boundConfig);

    return boundConfig;
  }
}
