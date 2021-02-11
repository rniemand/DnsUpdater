using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rn.DnsUpdater
{
  public class Worker : BackgroundService
  {
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
      _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      var request = new HttpRequestMessage(HttpMethod.Get, "https://api.ipify.org/");
      var httpClient = new HttpClient();
      var response = await httpClient.SendAsync(request, stoppingToken);
      var rawIpAddress = await response.Content.ReadAsStringAsync(stoppingToken);

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
