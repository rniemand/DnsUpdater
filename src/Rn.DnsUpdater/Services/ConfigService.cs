using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Config;
using Rn.DnsUpdater.Enums;
using Rn.NetCore.Common.Abstractions;
using Rn.NetCore.Common.Helpers;
using Rn.NetCore.Common.Logging;

namespace Rn.DnsUpdater.Services
{
  public class ConfigService : IConfigService
  {
    public DnsUpdaterConfig CoreConfig { get; }
    public DnsEntriesConfig DnsEntriesConfig { get; }

    private readonly ILoggerAdapter<ConfigService> _logger;
    private readonly IPathAbstraction _path;
    private readonly IDirectoryAbstraction _directory;
    private readonly IFileAbstraction _file;
    private readonly IJsonHelper _jsonHelper;
    private readonly IDateTimeAbstraction _dateTime;
    private readonly IEnvironmentAbstraction _environment;

    public ConfigService(IServiceProvider serviceProvider)
    {
      // TODO: [TESTS] (ConfigService) Add tests
      _logger = serviceProvider.GetRequiredService<ILoggerAdapter<ConfigService>>();
      _path = serviceProvider.GetRequiredService<IPathAbstraction>();
      _directory = serviceProvider.GetRequiredService<IDirectoryAbstraction>();
      _file = serviceProvider.GetRequiredService<IFileAbstraction>();
      _jsonHelper = serviceProvider.GetRequiredService<IJsonHelper>();
      _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();
      _environment = serviceProvider.GetRequiredService<IEnvironmentAbstraction>();

      // Load all required configuration
      CoreConfig = serviceProvider.GetRequiredService<DnsUpdaterConfig>();

      DnsEntriesConfig = LoadConfiguration(CoreConfig);
    }


    // Interface methods
    public List<DnsUpdaterEntry> GetEntriesNeedingUpdate()
    {
      // TODO: [TESTS] (ConfigService.GetEntriesNeedingUpdate) Add tests
      var now = _dateTime.Now;

      return DnsEntriesConfig.Entries
        .Where(e => NeedsUpdating(e, now))
        .ToList();
    }

    public List<DnsUpdaterEntry> GetEnabledEntries()
    {
      // TODO: [TESTS] (ConfigService.GetEnabledEntries) Add tests
      return DnsEntriesConfig.Entries.Where(e => e.Enabled).ToList();
    }

    public void SaveConfigState()
    {
      // TODO: [TESTS] (ConfigService.SaveConfigState) Add tests
      if (!ShiftConfigFiles())
      {
        _logger.Error("Unable to manage configuration files, quitting!");
        _environment.Exit(10);
      }

      try
      {
        var configJson = _jsonHelper.SerializeObject(DnsEntriesConfig, true);
        _file.WriteAllText(CoreConfig.ConfigFile, configJson);
        _logger.Debug("Updated configuration file");
      }
      catch (Exception ex)
      {
        _logger.LogUnexpectedException(ex);
        _environment.Exit(11);
      }
    }


    // Internal methods
    private bool ShiftConfigFiles()
    {
      // TODO: [TESTS] (ConfigService.ShiftConfigFiles) Add tests
      try
      {
        var previousConfig = $"{CoreConfig.ConfigFile}.previous";

        if (_file.Exists(previousConfig))
          _file.Delete(previousConfig);

        _file.Copy(CoreConfig.ConfigFile, previousConfig);
        _file.Delete(CoreConfig.ConfigFile);

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogUnexpectedException(ex);
        return false;
      }
    }

    private static bool NeedsUpdating(DnsUpdaterEntry entry, DateTime now)
    {
      // TODO: [TESTS] (ConfigService.NeedsUpdating) Add tests
      if (entry.Enabled == false)
        return false;

      if (!entry.NextUpdate.HasValue)
        return true;

      return !(entry.NextUpdate > now);
    }

    private DnsEntriesConfig LoadConfiguration(DnsUpdaterConfig config)
    {
      // TODO: [TESTS] (ConfigService.LoadConfiguration) Add tests
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
      // TODO: [TESTS] (ConfigService.GenerateSampleConfig) Add tests
      return new DnsEntriesConfig
      {
        Entries = new[]
        {
          new DnsUpdaterEntry
          {
            Type = DnsType.FreeDns,
            Enabled = false,
            NextUpdate = null,
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
