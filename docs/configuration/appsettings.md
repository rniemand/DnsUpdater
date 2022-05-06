[Home](/README.md) / [Docs](/docs/README.md) / [Configuration](/docs/configuration/README.md) / appsettings.json

# appsettings.json
More to come...

```json
{
  "configFile": "",
  "tickInterval": 5000,
  "updateHostIpIntervalMin": 10,
  "defaultHttpTimeoutMs": 5000
}
```

Details on each option is listed below.

| Property | Type | Required | Default | Notes |
| --- | --- | ---- | ---- | --- |
| `configFile` | Path | required | `./dns.config.json` | - |
| `tickInterval` | Int | optional | `5000` | Main loop tick interval in `ms`. |
| `updateHostIpIntervalMin` | Int | optional | `10` | Interval (in `min`) between checking for host IP Address change. |
| `defaultHttpTimeoutMs` | Int | optional | `5000` | Default HTTP Request timeout value in `ms`. |