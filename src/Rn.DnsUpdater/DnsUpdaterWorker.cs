using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Services;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater
{
  public class DnsUpdaterWorker : BackgroundService
  {
    private readonly ILoggerAdapter<DnsUpdaterWorker> _logger;
    private readonly IIpResolverService _resolverService;

    public DnsUpdaterWorker(
      ILoggerAdapter<DnsUpdaterWorker> logger,
      IIpResolverService resolverService,
      IFileAbstraction file,
      IEnvironmentAbstraction environment,
      DnsUpdaterConfig config)
    {
      _logger = logger;
      _resolverService = resolverService;


      if (file.Exists(config.ConfigFile))
      {
        _logger.Info(file.ReadAllText(config.ConfigFile));
      }
      else
      {
        _logger.Info("Unable to find configuration file");
      }

      _logger.Info("CurrentDirectory: {d}", environment.CurrentDirectory);

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
