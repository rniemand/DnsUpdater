using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Services;
using Rn.NetCore.BasicHttp;
using Rn.NetCore.BasicHttp.Factories;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;

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

      // Factories
      .AddSingleton<IHttpClientFactory, HttpClientFactory>()

      // Helpers
      .AddSingleton<IJsonHelper, JsonHelper>()

      // Logging
      .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))
      
      // Misc
      .AddSingleton<IDnsUpdateRunner, DnsUpdateRunner>()

      // Services
      .AddSingleton<IHostIpAddressService, HostIpAddressService>()
      .AddSingleton<IBasicHttpService, BasicHttpService>()
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
