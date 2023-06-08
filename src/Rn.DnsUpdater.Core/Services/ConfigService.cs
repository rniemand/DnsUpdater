using Microsoft.Extensions.DependencyInjection;
using Rn.DnsUpdater.Core.Config;
using Rn.DnsUpdater.Core.Enums;
using RnCore.Abstractions;
using RnCore.Logging;

namespace Rn.DnsUpdater.Core.Services;

public interface IConfigService
{
  DnsUpdaterConfig CoreConfig { get; }

  List<DnsUpdaterEntry> GetEntriesNeedingUpdate();
  List<DnsUpdaterEntry> GetEnabledEntries();
  void SaveConfigState();
}

public class ConfigService : IConfigService
{
  public DnsUpdaterConfig CoreConfig { get; }

  private readonly ILoggerAdapter<ConfigService> _logger;
  private readonly IPathAbstraction _path;
  private readonly IDirectoryAbstraction _directory;
  private readonly IFileAbstraction _file;
  private readonly IJsonHelper _jsonHelper;
  private readonly IDateTimeAbstraction _dateTime;
  private readonly IEnvironmentAbstraction _environment;
  private readonly DnsEntriesConfig _dnsEntriesConfig;

  public ConfigService(IServiceProvider serviceProvider)
  {
    _logger = serviceProvider.GetRequiredService<ILoggerAdapter<ConfigService>>();
    _path = serviceProvider.GetRequiredService<IPathAbstraction>();
    _directory = serviceProvider.GetRequiredService<IDirectoryAbstraction>();
    _file = serviceProvider.GetRequiredService<IFileAbstraction>();
    _jsonHelper = serviceProvider.GetRequiredService<IJsonHelper>();
    _dateTime = serviceProvider.GetRequiredService<IDateTimeAbstraction>();
    _environment = serviceProvider.GetRequiredService<IEnvironmentAbstraction>();
    CoreConfig = serviceProvider.GetRequiredService<DnsUpdaterConfig>();

    _dnsEntriesConfig = LoadConfiguration(CoreConfig);
  }


  // Interface methods
  public List<DnsUpdaterEntry> GetEntriesNeedingUpdate()
  {
    var now = _dateTime.Now;

    return _dnsEntriesConfig.Entries
      .Where(e => NeedsUpdating(e, now))
      .ToList();
  }

  public List<DnsUpdaterEntry> GetEnabledEntries()
  {
    return _dnsEntriesConfig.Entries.Where(e => e.Enabled).ToList();
  }

  public void SaveConfigState()
  {
    if (!ShiftConfigFiles())
    {
      _logger.LogError("Unable to manage configuration files, quitting!");
      // TODO: [COMPLETE] (ConfigService.SaveConfigState) complete this
      Environment.Exit(10);
    }

    try
    {
      var configJson = _jsonHelper.SerializeObject(_dnsEntriesConfig, true);
      _file.WriteAllText(CoreConfig.ConfigFile, configJson);
      _logger.LogDebug("Updated configuration file");
    }
    catch (Exception ex)
    {
      // TODO: [LOGGING] (ConfigService.SaveConfigState) Replace method call
      //_logger.LogUnexpectedException(ex);
      _logger.LogError(ex, ex.Message);
      // TODO: [COMPLETE] (ConfigService.SaveConfigState) complete this
      //_environment.Exit(11);
      Environment.Exit(11);
    }
  }


  // Internal methods
  private bool ShiftConfigFiles()
  {
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
      // TODO: [LOGGING] (ConfigService.ShiftConfigFiles) Replace this call
      //_logger.LogUnexpectedException(ex);
      _logger.LogError(ex, ex.Message);
      return false;
    }
  }

  private static bool NeedsUpdating(DnsUpdaterEntry entry, DateTime now)
  {
    if (!entry.Enabled)
      return false;

    if (!entry.NextUpdate.HasValue)
      return true;

    return !(entry.NextUpdate > now);
  }

  private DnsEntriesConfig LoadConfiguration(DnsUpdaterConfig config)
  {
    var configDir = _path.GetDirectoryName(config.ConfigFile);

    // Ensure that the configuration directory exists
    if (!_directory.Exists(configDir))
    {
      _logger.LogInformation("Creating configuration directory: {path}", configDir);
      _directory.CreateDirectory(configDir);
    }

    // Ensure that we have a configuration file to work with
    if (!_file.Exists(config.ConfigFile))
    {
      _logger.LogInformation("Generating sample configuration file: {path}", config.ConfigFile);
      var sampleJson = _jsonHelper.SerializeObject(GenerateSampleConfig(), true);
      _file.WriteAllText(config.ConfigFile, sampleJson);
    }

    // Load, parse and return our configuration
    var rawConfig = _file.ReadAllText(config.ConfigFile);
    return _jsonHelper.DeserializeObject<DnsEntriesConfig>(rawConfig);
  }

  private static DnsEntriesConfig GenerateSampleConfig()
  {
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
