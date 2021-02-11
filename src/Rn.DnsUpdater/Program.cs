using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rn.DnsUpdater.Services;

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
            .AddHostedService<Worker>();
        });
  }
}
