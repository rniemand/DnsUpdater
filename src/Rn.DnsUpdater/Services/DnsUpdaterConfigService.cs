using Rn.DnsUpdater.Config;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Services
{
  public interface IDnsUpdaterConfigService
  {
  }

  public class DnsUpdaterConfigService : IDnsUpdaterConfigService
  {
    private readonly ILoggerAdapter<DnsUpdaterConfigService> _logger;
    private readonly IPathAbstraction _path;
    private readonly IDirectoryAbstraction _directory;
    private readonly IFileAbstraction _file;
    private readonly IJsonHelper _jsonHelper;

    public DnsUpdaterConfigService(
      ILoggerAdapter<DnsUpdaterConfigService> logger,
      IPathAbstraction path,
      IDirectoryAbstraction directory,
      IFileAbstraction file,
      IJsonHelper jsonHelper,
      DnsUpdaterConfig config)
    {
      // TODO: [TESTS] (DnsUpdaterConfigService) Add tests
      _logger = logger;
      _path = path;
      _directory = directory;
      _file = file;
      _jsonHelper = jsonHelper;


      var configDir = path.GetDirectoryName(config.ConfigFile);

      if (!directory.Exists(configDir))
      {
        directory.CreateDirectory(configDir);
      }

      if (!file.Exists(config.ConfigFile))
      {
        var sampleConfig = new DnsEntriesConfig();
        var sampleConfigJson = _jsonHelper.SerializeObject(sampleConfig, true);
        file.WriteAllText(config.ConfigFile, sampleConfigJson);
      }



    }


  }
}
