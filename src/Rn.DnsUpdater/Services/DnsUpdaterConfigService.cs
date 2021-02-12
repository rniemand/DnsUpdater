﻿using System.Collections.Generic;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Services
{
  public interface IDnsUpdaterConfigService
  {
    DnsUpdaterConfig CoreConfig { get; }
    DnsEntriesConfig DnsEntriesConfig { get; }
  }

  public class DnsUpdaterConfigService : IDnsUpdaterConfigService
  {
    public DnsUpdaterConfig CoreConfig { get; set; }
    public DnsEntriesConfig DnsEntriesConfig { get; set; }

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

      // Load all required configuration
      CoreConfig = config;
      DnsEntriesConfig = LoadConfiguration(config);
    }


    // Internal methods
    private DnsEntriesConfig LoadConfiguration(DnsUpdaterConfig config)
    {
      // TODO: [TESTS] (DnsUpdaterConfigService.LoadConfiguration) Add tests
      var configDir = _path.GetDirectoryName(config.ConfigFile);

      // Ensure that the configuration directory exists
      if (!_directory.Exists(configDir))
      {
        _logger.Info("Creating configuration directory: {path}", configDir);
        _directory.CreateDirectory(configDir);
      }

      // Ensure that we have a configuration file to work with
      if (!_file.Exists(config.ConfigFile))
      {
        _logger.Info("Generating sample configuration file: {path}", config.ConfigFile);
        var sampleJson = _jsonHelper.SerializeObject(GenerateSampleConfig(), true);
        _file.WriteAllText(config.ConfigFile, sampleJson);
      }

      // Load, parse and return our configuration
      var rawConfig = _file.ReadAllText(config.ConfigFile);
      return _jsonHelper.DeserializeObject<DnsEntriesConfig>(rawConfig);
    }

    private static DnsEntriesConfig GenerateSampleConfig()
    {
      // TODO: [TESTS] (DnsUpdaterConfigService.GenerateSampleConfig) Add tests
      return new DnsEntriesConfig
      {
        Entries = new[]
        {
          new DnsUpdaterEntry
          {
            Type = DnsType.FreeDns,
            Enabled = false,
            Config = new Dictionary<string, string>
            {
              {ConfigKeys.Url, "http://foobar.com"}
            }
          }
        }
      };
    }
  }
}
