using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rn.DnsUpdater.Services;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;

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
            .AddSingleton<IIpResolverService, IpResolverService>()
            .AddSingleton(typeof(ILoggerAdapter<>), typeof(LoggerAdapter<>))
            .AddSingleton<IFileAbstraction, FileAbstraction>()
            .AddSingleton<IDirectoryAbstraction, DirectoryAbstraction>()
            .AddSingleton<IEnvironmentAbstraction, EnvironmentAbstraction>()
            .AddHostedService<DnsUpdaterWorker>();
        });
  }
}
