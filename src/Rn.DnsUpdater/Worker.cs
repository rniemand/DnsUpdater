using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Rn.DnsUpdater.Services;

namespace Rn.DnsUpdater
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;
    private readonly IIpResolverService _resolverService;

    public Worker(
      ILogger<Worker> logger,
      IIpResolverService resolverService)
    {
      _logger = logger;
      _resolverService = resolverService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var rawIpAddress = await _resolverService.GetIpAddress(stoppingToken);

      while (!stoppingToken.IsCancellationRequested)
      {
        _logger.LogInformation(
          "Worker running at: {time} (my IP Address: {ip})",
          DateTimeOffset.Now,
          rawIpAddress
        );

        await Task.Delay(5000, stoppingToken);
      }
    }
  }
}
