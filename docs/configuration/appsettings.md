# appsettings.json
More to come...

```json
{
  "configFile": "",
  "tickInterval": 5000,
  "updateHostIpIntervalMin": 10,
  "defaultHttpTimeoutMs": 5000,
  "providerUrls": {
    "ipify": "https://api.ipify.org/"
  }
}
```

Details on each option is listed below.

| Property | Type | Required | Default | Notes |
| --- | --- | ---- | ---- | --- |
| `configFile` | Path | required | `./dns.config.json` | - |
| `tickInterval` | Int | optional | `5000` | Main loop tick interval in `ms`. |
| `updateHostIpIntervalMin` | Int | optional | `10` | Interval (in `min`) between checking for host IP Address change. |
| `defaultHttpTimeoutMs` | Int | optional | `5000` | Default HTTP Request timeout value in `ms`. |
| `providerUrls` | `Dictionary<>` | required | `{}` | Lookup table for IP Address providers. |

## providerUrls
The following provider URL values need to be defined:

- `ipify`: with a value of "https://api.ipify.org/"