using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Rn.DnsUpdater.Core.Extensions;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Rn.DnsUpdater;

public class Program
{
  public static void Main(string[] args)
  {
    var logger = LogManager.GetCurrentClassLogger();

    try
    {
      CreateHostBuilder(args).Build().Run();
    }
    catch (Exception ex)
    {
      logger.Error(ex, "Stopped program because of exception");
      throw;
    }
    finally
    {
      LogManager.Shutdown();
    }
  }

  public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
      .ConfigureServices((hostContext, services) =>
      {
        services
          .AddRnDnsUpdater(hostContext.Configuration)
          .AddHostedService<DnsUpdaterWorker>()
          .AddLogging(loggingBuilder =>
          {
            // configure Logging with NLog
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.AddNLog(hostContext.Configuration);
          });
      });
}
