using System;
using System.Collections.Generic;
using System.Linq;
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

    List<DnsUpdaterEntry> GetEntriesNeedingUpdate();
    void SaveConfigState();
  }

  public class DnsUpdaterConfigService : IDnsUpdaterConfigService
  {
    public DnsUpdaterConfig CoreConfig { get; private set; }
    public DnsEntriesConfig DnsEntriesConfig { get; private set; }

    private readonly ILoggerAdapter<DnsUpdaterConfigService> _logger;
    private readonly IPathAbstraction _path;
    private readonly IDirectoryAbstraction _directory;
    private readonly IFileAbstraction _file;
    private readonly IJsonHelper _jsonHelper;
    private readonly IDateTimeAbstraction _dateTime;
    private readonly IEnvironmentAbstraction _environment;

    public DnsUpdaterConfigService(
      ILoggerAdapter<DnsUpdaterConfigService> logger,
      IPathAbstraction path,
      IDirectoryAbstraction directory,
      IFileAbstraction file,
      IJsonHelper jsonHelper,
      IDateTimeAbstraction dateTime,
      IEnvironmentAbstraction environment,
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
      _environment = environment;
      _dateTime = dateTime;
      DnsEntriesConfig = LoadConfiguration(config);
    }


    // Interface methods
    public List<DnsUpdaterEntry> GetEntriesNeedingUpdate()
    {
      // TODO: [TESTS] (DnsUpdaterConfigService.GetEntriesNeedingUpdate) Add tests
      var now = _dateTime.Now;

      return DnsEntriesConfig.Entries
        .Where(e => NeedsUpdating(e, now))
        .ToList();
    }

    public void SaveConfigState()
    {
      // TODO: [TESTS] (DnsUpdaterConfigService.SaveConfigState) Add tests
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
      // TODO: [TESTS] (DnsUpdaterConfigService.ShiftConfigFiles) Add tests
      try
      {
        var previousConfig = $"{CoreConfig.ConfigFile}.previous";

        if(_file.Exists(previousConfig))
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
      // TODO: [TESTS] (DnsUpdaterConfigService.NeedsUpdating) Add tests
      if (entry.Enabled == false)
        return false;

      if (!entry.NextUpdate.HasValue)
        return true;

      return !(entry.NextUpdate > now);
    }

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
