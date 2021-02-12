using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Services;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Common.Services;

namespace Rn.DnsUpdater
{
  public class Program
  {
    public static void Main(string[] args)
    {
      CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .ConfigureServices((hostContext, services) =>
        {
          services
            .AddSingleton(GenerateConfig(hostContext))
            .AddSingleton<IIpResolverService, IpResolverService>()
            .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))
            .AddSingleton<IFileAbstraction, FileAbstraction>()
            .AddSingleton<IDirectoryAbstraction, DirectoryAbstraction>()
            .AddSingleton<IEnvironmentAbstraction, EnvironmentAbstraction>()
            .AddSingleton<IPathAbstraction, PathAbstraction>()
            .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()
            .AddSingleton<IJsonHelper, JsonHelper>()
            .AddSingleton<IBasicHttpService, BasicHttpService>()
            .AddSingleton<IDnsUpdaterService, DnsUpdaterService>()
            .AddSingleton<IDnsUpdaterConfigService, DnsUpdaterConfigService>()
            .AddHostedService<DnsUpdaterWorker>();
        });

    // Helper methods
    private static DnsUpdaterConfig GenerateConfig(HostBuilderContext hostContext)
    {
      // TODO: [TESTS] (Program.GenerateConfig) Add tests
      var boundConfig = new DnsUpdaterConfig();
      var section = hostContext.Configuration.GetSection("DnsUpdater");

      if (section.Exists())
        section.Bind(boundConfig);

      return boundConfig;
    }
  }
}
