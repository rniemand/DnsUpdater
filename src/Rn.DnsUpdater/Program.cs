using System;
using DocumentSink.ClientLib;
using DocumentSink.ClientLib.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Services;
using Rn.NetCore.BasicHttp;
using Rn.NetCore.BasicHttp.Factories;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;
using Rn.NetCore.Metrics;
using Rn.NetCore.Metrics.Outputs;
using Rn.NetCore.Metrics.Rabbit;
using Rn.NetCore.Metrics.Rabbit.Interfaces;
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
          // Config
          .AddSingleton(GenerateConfig(hostContext))
          .AddSingleton(hostContext.Configuration)

          // Clients
          .AddSingleton<IDocumentSinkClient, DocumentSinkClient>()

          // Services
          .AddSingleton<IHostIpAddressService, HostIpAddressService>()
          .AddSingleton<IBasicHttpService, BasicHttpService>()
          .AddSingleton<IDnsUpdaterService, DnsUpdaterService>()
          .AddSingleton<IConfigService, ConfigService>()
          .AddSingleton<IHeartbeatService, HeartbeatService>()

          // Abstractions
          .AddSingleton<IFileAbstraction, FileAbstraction>()
          .AddSingleton<IDirectoryAbstraction, DirectoryAbstraction>()
          .AddSingleton<IEnvironmentAbstraction, EnvironmentAbstraction>()
          .AddSingleton<IPathAbstraction, PathAbstraction>()
          .AddSingleton<IDateTimeAbstraction, DateTimeAbstraction>()

          // Factories
          .AddSingleton<IHttpClientFactory, HttpClientFactory>()

          // Helpers
          .AddSingleton<IJsonHelper, JsonHelper>()

          // Metrics
          .AddSingleton<IMetricService, MetricService>()
          .AddSingleton<IMetricServiceUtils, MetricServiceUtils>()
          .AddSingleton<IMetricOutput, RabbitMetricOutput>()
          .AddSingleton<IRabbitConnection, RabbitConnection>()
          .AddSingleton<IRabbitFactory, RabbitFactory>()

          // Logging
          .AddSingleton(typeof(ILoggerAdapter<>), typeof(DocumentSinkLoggerAdapter<>))
          .AddLogging(loggingBuilder =>
          {
            // configure Logging with NLog
            loggingBuilder.ClearProviders();
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
            loggingBuilder.AddNLog(hostContext.Configuration);
          })

          // Workers
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