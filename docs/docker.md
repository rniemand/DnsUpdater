# Docker

You can view this project on Docker Hub [here](https://hub.docker.com/repository/docker/niemandr/rndnsupdater).

```text
CORE
  NAME:    DNSUpdater
  IMAGE:   niemandr/rndnsupdater
  HUB URL: https://hub.docker.com/repository/docker/niemandr/rndnsupdater

PATHS
  /config                /mnt/user/Backups/app-data/dns-updater/
  /logs                  /mnt/user/appdata/logs/dns-updater/
  /app/nlog.config       /mnt/user/Backups/app-data/dns-updater/nlog.config
  /app/appsettings.json  /mnt/user/Backups/app-data/dns-updater/appsettings.json

MISC
  LOGO  https://raw.githubusercontent.com/rniemand/DnsUpdater/master/resources/logo.png
```

## Custom Files

### nlog.config

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
      autoReload="true" >

  <targets>
    <target xsi:type="Console"
            name="console"
            layout="${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}" />
    
    <target xsi:type="File"
            name="file"
            fileName="/logs/dns-updater.log"
            layout="${date}|${level:uppercase=true}|${message} ${exception}|${logger}|${all-event-properties}" />
  </targets>

  <rules>
    <logger name="*" minlevel="Trace" writeTo="console,file" />
  </rules>
</nlog>
```

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "DnsUpdater": {
    "configFile": "/config/dns.config.json",
    "tickInterval": 5000,
    "updateHostIpIntervalMin": 10,
    "defaultHttpTimeoutMs": 5000,
    "providerUrls": {
      "ipify": "https://api.ipify.org/"
    }
  },
  "RnCore.Metrics": {
    "application": "DnsUpdater",
    "enabled": true,
    "enableConsoleOutput": true,
    "environment": "dev",
    "template": "{app}/{measurement}"
  },
  "RnCore.Metrics.InfluxDb": {
    "token": "...",
    "bucket": "default",
    "org": "...",
    "url": "http://192.168.0.60:8086",
    "enabled": true
  }
}
```

### dns.config.json

```json
{
  "entries": [
    {
      "enabled": true,
      "name": "a.xxx.ca",
      "type": 1,
      "updateIntervalSec": 600,
      "config": {
        "Url": "..."
      }
    }
  ]
}
```