using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Services;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater
{
  public class Worker : BackgroundService
  {
    private readonly ILoggerAdapter<Worker> _logger;
    private readonly IIpResolverService _resolverService;

    public Worker(
      ILoggerAdapter<Worker> logger,
      IIpResolverService resolverService,
      IConfiguration configuration,
      IFileAbstraction file)
    {
      _logger = logger;
      _resolverService = resolverService;


      var config = new DnsUpdaterConfig();
      var configSection = configuration.GetSection("DnsUpdater");
      if (configSection.Exists())
      {
        configSection.Bind(config);
      }

      if (file.Exists(config.ConfigFile))
      {
        _logger.Info(file.ReadAllText(config.ConfigFile));
      }
      else
      {
        _logger.Info("Unable to find configuration file");
      }



    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var rawIpAddress = await _resolverService.GetIpAddress(stoppingToken);

      while (!stoppingToken.IsCancellationRequested)
      {
        _logger.Info(
          "Worker running at: {time} (my IP Address: {ip})",
          DateTimeOffset.Now,
          rawIpAddress
        );

        await Task.Delay(5000, stoppingToken);
      }
    }
  }
}
